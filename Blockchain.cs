using System;
using System.Collections.Generic;
using System.Linq;
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

            int congestionMultiplier = (int)Math.Ceiling(
                (decimal)mempoolSizeBytes / MaxBlockSizeBytes
            );

            return BaseFeePerByte * congestionMultiplier;
        }

        public decimal GetBalance(string address)
        {
            decimal balance = 0;

            foreach (Block block in Chain)
            {
                foreach (Transaction transaction in block.Transactions)
                {
                    if (transaction.To == address)
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

                    if (transaction.From == address)
                    {
                        balance -= transaction.Amount;
                        balance -= transaction.Fee;
                    }
                }
            }

            return balance;
        }

        public decimal GetPendingBalance(string address)
        {
            decimal balance = GetBalance(address);

            foreach (Transaction transaction in PendingTransactions)
            {
                if (transaction.From == address)
                {
                    balance -= transaction.Amount;
                    balance -= transaction.Fee;
                }

                if (transaction.To == address && transaction.From != "COINBASE")
                {
                    balance += transaction.Amount;
                }
            }

            return balance;
        }

        public void AddTransaction(Transaction transaction)
        {
            if (!transaction.IsValid())
            {
                throw new Exception("Транзакція не пройшла перевірку підпису");
            }

            decimal currentFeePerByte = GetCurrentNetworkFee();
            decimal minimumRequiredFee = transaction.Size * currentFeePerByte;

            if (transaction.Fee < minimumRequiredFee)
            {
                throw new Exception(
                    $"Комісія занадто мала. Мінімум: {minimumRequiredFee:F2}, " +
                    $"поточний тариф: {currentFeePerByte:F2} за байт"
                );
            }

            if (!string.IsNullOrWhiteSpace(transaction.ReplacesTxId))
            {
                Transaction? oldTransaction = PendingTransactions
                    .FirstOrDefault(pendingTransaction => pendingTransaction.Id == transaction.ReplacesTxId);

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
                PendingTransactions.Add(transaction);

                return;
            }

            PendingTransactions.Add(transaction);
        }

        private void CreateGenesisBlock()
        {
            Block genesisBlock = new Block(
                index: 0,
                previousHash: "0",
                transactions: new List<Transaction>(),
                difficulty: Difficulty
            );

            genesisBlock.Hash = "GENESIS_HASH";

            Chain.Add(genesisBlock);
        }

        public void AddBlock(List<Transaction> transactions)
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
                    Console.WriteLine("Transaction rejected.");
                }
            }

            if (validTransactions.Count == 0)
            {
                Console.WriteLine("No valid transactions for block.");
                return;
            }

            Block previousBlock = Chain[^1];

            Block newBlock = new Block(
                index: Chain.Count,
                previousHash: previousBlock.Hash,
                transactions: validTransactions,
                difficulty: Difficulty
            );

            Miner miner = new Miner();

            MiningResult result = miner.Mine(newBlock);

            newBlock.Nonce = result.Nonce;
            newBlock.Hash = result.Hash;

            Chain.Add(newBlock);

            foreach (Transaction transaction in validTransactions)
            {
                PendingTransactions.Remove(transaction);
            }

            Console.WriteLine($"Block #{newBlock.Index} added.");
        }
    }
}
