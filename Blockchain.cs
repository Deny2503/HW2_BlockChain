using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class Blockchain
    {
        public List<Block> Chain { get; }
        public List<Transaction> PendingTransactions { get; }

        public decimal BaseFeePerByte { get; } = 0.05m;
        public int MaxBlockSizeBytes { get; } = 1024;
        public int CoinbaseMaturity { get; set; } = 3;
        public int MaxReorgDepth { get; set; } = 5;
        public decimal MiningReward { get; set; } = 50m;

        private const int Difficulty = 4;

        public Blockchain()
        {
            Chain = new List<Block>();
            PendingTransactions = new List<Transaction>();
            CreateGenesisBlock();
        }

        public decimal GetCurrentNetworkFee()
        {
            int mempoolSizeBytes = PendingTransactions.Sum(transaction => transaction.Size);
            if (mempoolSizeBytes <= MaxBlockSizeBytes)
            {
                return BaseFeePerByte;
            }

            int congestionMultiplier = (int)Math.Ceiling((decimal)mempoolSizeBytes / MaxBlockSizeBytes);
            return BaseFeePerByte * congestionMultiplier;
        }

        public decimal GetBalance(string address, string tokenSymbol = "MAIN")
        {
            tokenSymbol = NormalizeToken(tokenSymbol);
            decimal balance = 0;

            foreach (Block block in Chain)
            {
                foreach (Transaction transaction in block.Transactions)
                {
                    if (transaction.TokenSymbol == tokenSymbol && transaction.To == address)
                    {
                        if (transaction.From == "COINBASE")
                        {
                            int confirmations = Chain.Count - block.Index;
                            if (confirmations >= CoinbaseMaturity)
                            {
                                balance += transaction.Amount;
                            }
                        }
                        else
                        {
                            balance += transaction.Amount;
                        }
                    }

                    if (transaction.TokenSymbol == tokenSymbol && transaction.From == address)
                    {
                        balance -= transaction.Amount;
                    }

                    if (tokenSymbol == "MAIN" && transaction.From == address)
                    {
                        balance -= transaction.Fee;
                    }
                }
            }

            return balance;
        }

        public decimal GetPendingBalance(string address, string tokenSymbol = "MAIN")
        {
            tokenSymbol = NormalizeToken(tokenSymbol);
            decimal balance = GetBalance(address, tokenSymbol);

            foreach (Transaction transaction in PendingTransactions)
            {
                if (transaction.TokenSymbol == tokenSymbol && transaction.To == address && transaction.From != "COINBASE")
                {
                    balance += transaction.Amount;
                }

                if (transaction.TokenSymbol == tokenSymbol && transaction.From == address)
                {
                    balance -= transaction.Amount;
                }

                if (tokenSymbol == "MAIN" && transaction.From == address)
                {
                    balance -= transaction.Fee;
                }
            }

            return balance;
        }

        public List<string> GetKnownTokenSymbols()
        {
            return Chain.SelectMany(block => block.Transactions)
                .Concat(PendingTransactions)
                .Select(transaction => transaction.TokenSymbol)
                .Append("MAIN")
                .Distinct()
                .OrderBy(symbol => symbol)
                .ToList();
        }

        public void AddTransaction(Transaction transaction)
        {
            transaction.TokenSymbol = NormalizeToken(transaction.TokenSymbol);

            if (!transaction.IsValid())
            {
                throw new Exception("Транзакція не пройшла перевірку підпису");
            }

            if (transaction.From == "COINBASE")
            {
                throw new Exception("COINBASE транзакції створюються тільки під час майнінгу");
            }

            if (transaction.From == "MINT")
            {
                if (transaction.TokenSymbol == "MAIN")
                {
                    throw new Exception("Не можна випускати MAIN через Mint");
                }

                PendingTransactions.Add(transaction);
                return;
            }

            decimal currentFeePerByte = GetCurrentNetworkFee();
            decimal minimumRequiredFee = transaction.Size * currentFeePerByte;
            if (transaction.Fee < minimumRequiredFee)
            {
                throw new Exception($"Комісія занадто мала. Мінімум: {minimumRequiredFee:F2}, поточний тариф: {currentFeePerByte:F2} за байт");
            }

            decimal tokenBalance = GetPendingBalance(transaction.From, transaction.TokenSymbol);
            decimal mainBalance = GetPendingBalance(transaction.From, "MAIN");

            if (tokenBalance < transaction.Amount)
            {
                throw new Exception($"Недостатньо токена {transaction.TokenSymbol}. Доступно: {tokenBalance:F2}, потрібно: {transaction.Amount:F2}");
            }

            if (mainBalance < transaction.Fee)
            {
                throw new Exception($"Недостатньо MAIN для комісії. Доступно: {mainBalance:F2}, потрібно: {transaction.Fee:F2}");
            }

            if (!string.IsNullOrWhiteSpace(transaction.ReplacesTxId))
            {
                Transaction? oldTransaction = PendingTransactions.FirstOrDefault(p => p.Id == transaction.ReplacesTxId);
                if (oldTransaction == null)
                {
                    throw new Exception("Стару транзакцію не знайдено у Mempool");
                }

                if (oldTransaction.From != transaction.From)
                {
                    throw new Exception("Замінити транзакцію може тільки її відправник");
                }

                if (transaction.Fee <= oldTransaction.Fee)
                {
                    throw new Exception("Нова комісія має бути вищою за стару");
                }

                PendingTransactions.Remove(oldTransaction);
            }

            PendingTransactions.Add(transaction);
        }

        public void MinePendingTransactions(string minerAddress)
        {
            List<Transaction> transactions = new List<Transaction>();
            transactions.Add(new Transaction("COINBASE", minerAddress, MiningReward, "", 0, "MAIN"));
            transactions.AddRange(PendingTransactions.OrderByDescending(transaction => transaction.Fee).ToList());
            AddBlock(transactions, minerAddress);
        }

        public void AddBlock(List<Transaction> transactions, string minerAddress = "")
        {
            List<Transaction> validTransactions = new();
            foreach (Transaction transaction in transactions)
            {
                if (transaction.IsValid())
                {
                    validTransactions.Add(transaction);
                }
                else
                {
                    Console.WriteLine("Transaction rejected: invalid signature.");
                }
            }

            if (validTransactions.Count == 0)
            {
                Console.WriteLine("No valid transactions for block.");
                return;
            }

            Block previousBlock = Chain[^1];
            Block newBlock = new Block(Chain.Count, previousBlock.Hash, validTransactions, Difficulty, minerAddress);

            if (newBlock.Timestamp <= previousBlock.Timestamp)
            {
                newBlock.Timestamp = previousBlock.Timestamp + 1;
            }

            Miner miner = new Miner();
            MiningResult result = miner.Mine(newBlock);
            newBlock.Nonce = result.Nonce;
            newBlock.Hash = result.Hash;

            Chain.Add(newBlock);

            foreach (Transaction transaction in validTransactions)
            {
                PendingTransactions.RemoveAll(pending => pending.Id == transaction.Id);
            }

            Console.WriteLine($"Block #{newBlock.Index} added. Miner: {minerAddress}");
        }

        public bool TryAddBlockFromPeer(Block peerBlock)
        {
            if (!IsValidPeerBlock(peerBlock))
            {
                Console.WriteLine("[P2P] Peer block rejected: invalid hash, index, previous hash, timestamp, structure or Proof of Work.");
                return false;
            }

            Chain.Add(peerBlock);
            foreach (Transaction transaction in peerBlock.Transactions)
            {
                PendingTransactions.RemoveAll(pending => pending.Id == transaction.Id);
            }

            Console.WriteLine($"[P2P] Peer block #{peerBlock.Index} added to chain.");
            return true;
        }

        public bool IsChainValid(List<Block>? chainToValidate = null)
        {
            List<Block> chain = chainToValidate ?? Chain;
            if (chain.Count == 0 || chain[0].Hash != "GENESIS_HASH")
            {
                return false;
            }

            for (int i = 1; i < chain.Count; i++)
            {
                Block previousBlock = chain[i - 1];
                Block currentBlock = chain[i];

                if (currentBlock.Index != previousBlock.Index + 1) return false;
                if (currentBlock.PreviousHash != previousBlock.Hash) return false;
                if (!HasValidTimestamp(currentBlock, previousBlock)) return false;
                if (!HasValidBlockStructureAndProof(currentBlock)) return false;
            }

            return true;
        }

        public bool ResolveConflicts(List<Block> candidateChain)
        {
            if (candidateChain.Count <= Chain.Count)
            {
                Console.WriteLine("Відхилено: Отриманий ланцюг не довший за локальний.");
                return false;
            }

            if (!IsChainValid(candidateChain))
            {
                Console.WriteLine("Відхилено: Отриманий ланцюг не пройшов валідацію.");
                return false;
            }

            Block? forkPoint = FindForkPoint(candidateChain);
            if (forkPoint == null)
            {
                Console.WriteLine("Відхилено: Не знайдено спільну точку розгалуження.");
                return false;
            }

            int reorgDepth = Chain[^1].Index - forkPoint.Index;
            if (reorgDepth > MaxReorgDepth)
            {
                Console.WriteLine("Відхилено: Спроба глибокої реорганізації. Локальні блоки вже фіналізовані.");
                return false;
            }

            Chain.Clear();
            Chain.AddRange(candidateChain);
            Console.WriteLine("Консенсус прийнято: локальний ланцюг замінено довшим валідним ланцюгом.");
            return true;
        }

        private Block? FindForkPoint(List<Block> candidateChain)
        {
            int maxIndex = Math.Min(Chain.Count, candidateChain.Count) - 1;
            for (int i = maxIndex; i >= 0; i--)
            {
                if (Chain[i].Hash == candidateChain[i].Hash)
                {
                    return Chain[i];
                }
            }
            return null;
        }

        private bool IsValidPeerBlock(Block peerBlock)
        {
            if (peerBlock == null) return false;
            Block previousBlock = Chain[^1];
            if (peerBlock.Index != previousBlock.Index + 1) return false;
            if (peerBlock.PreviousHash != previousBlock.Hash) return false;
            if (!HasValidTimestamp(peerBlock, previousBlock)) return false;
            return HasValidBlockStructureAndProof(peerBlock);
        }

        private bool HasValidTimestamp(Block currentBlock, Block previousBlock)
        {
            long maxAllowedTimestamp = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds();
            if (currentBlock.Timestamp <= previousBlock.Timestamp) return false;
            if (currentBlock.Timestamp > maxAllowedTimestamp) return false;
            return true;
        }

        private bool HasValidBlockStructureAndProof(Block block)
        {
            if (block.Transactions == null || block.Transactions.Count == 0) return false;
            if (block.Transactions.Any(transaction => !transaction.IsValid())) return false;

            string recalculatedHash = CalculateBlockHash(block, block.Nonce);
            if (block.Hash != recalculatedHash) return false;

            try
            {
                byte[] hashBytes = Convert.FromHexString(block.Hash);
                return HashValidator.IsValid(hashBytes, block.Difficulty);
            }
            catch
            {
                return false;
            }
        }

        private string CalculateBlockHash(Block block, int nonce)
        {
            byte[] prefixBytes = Encoding.UTF8.GetBytes(block.PreviousHash + GetTransactionsText(block.Transactions) + block.Timestamp);
            byte[] buffer = new byte[prefixBytes.Length + 4];
            Array.Copy(prefixBytes, buffer, prefixBytes.Length);
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(prefixBytes.Length, 4), nonce);
            byte[] hashBytes = SHA256.HashData(buffer);
            return HashConverter.ToHex(hashBytes);
        }

        private string GetTransactionsText(List<Transaction> transactions)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Transaction transaction in transactions)
            {
                builder.Append(transaction.From);
                builder.Append(transaction.To);
                builder.Append(transaction.Amount);
                builder.Append(transaction.Fee);
                builder.Append(transaction.TokenSymbol);
                builder.Append(transaction.Timestamp);
            }
            return builder.ToString();
        }

        private void CreateGenesisBlock()
        {
            Block genesisBlock = new Block(0, "0", new List<Transaction>(), Difficulty, "GENESIS");
            genesisBlock.Hash = "GENESIS_HASH";
            Chain.Add(genesisBlock);
        }

        private static string NormalizeToken(string tokenSymbol)
        {
            return string.IsNullOrWhiteSpace(tokenSymbol) ? "MAIN" : tokenSymbol.Trim().ToUpperInvariant();
        }
    }
}
