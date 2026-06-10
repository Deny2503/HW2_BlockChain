using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class BlockchainExplorerService
    {
        private readonly Blockchain blockchain;

        public BlockchainExplorerService(Blockchain blockchain)
        {
            this.blockchain = blockchain;
        }

        public Transaction? FindTransactionById(string txId)
        {
            return blockchain.Chain
                .SelectMany(block => block.Transactions)
                .Concat(blockchain.PendingTransactions)
                .FirstOrDefault(transaction => transaction.Id == txId);
        }

        public Block? FindBlockByTransactionId(string txId)
        {
            return blockchain.Chain
                .FirstOrDefault(block => block.Transactions.Any(transaction => transaction.Id == txId));
        }

        public List<Transaction> GetTransactionHistory(string address)
        {
            return blockchain.Chain
                .SelectMany(block => block.Transactions)
                .Concat(blockchain.PendingTransactions)
                .Where(transaction => transaction.From == address || transaction.To == address)
                .OrderByDescending(transaction => transaction.Timestamp)
                .ToList();
        }

        public decimal GetTotalFeesEarned(string minerAddress)
        {
            return blockchain.Chain
                .Where(block => block.MinerAddress == minerAddress)
                .SelectMany(block => block.Transactions)
                .Where(transaction => transaction.From != "COINBASE" && transaction.From != "MINT")
                .Sum(transaction => transaction.Fee);
        }
    }
}
