using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class Display
    {
        private readonly Blockchain blockchain;

        public Display(Blockchain blockchain)
        {
            this.blockchain = blockchain;
        }

        public void PrintTransactionHistory(string address)
        {
            Console.WriteLine();
            Console.WriteLine($"===== Transaction history for {address} =====");

            bool found = false;

            for (int i = 1; i < blockchain.Chain.Count; i++)
            {
                Block block = blockchain.Chain[i];

                foreach (Transaction transaction in block.Transactions)
                {
                    if (transaction.From == address || transaction.To == address)
                    {
                        found = true;

                        Console.WriteLine(
                            $"Block #{block.Index} | " +
                            $"{transaction.From} -> {transaction.To} | " +
                            $"Amount: {transaction.Amount}"
                        );
                    }
                }
            }

            if (!found)
            {
                Console.WriteLine("Transactions not found.");
            }
        }

        public void PrintBiggestTransaction()
        {
            Transaction? biggestTransaction = null;
            int blockNumber = -1;

            for (int i = 1; i < blockchain.Chain.Count; i++)
            {
                Block block = blockchain.Chain[i];

                foreach (Transaction transaction in block.Transactions)
                {
                    if (biggestTransaction == null ||
                        transaction.Amount > biggestTransaction.Amount)
                    {
                        biggestTransaction = transaction;
                        blockNumber = block.Index;
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("===== Whale Tracker =====");

            if (biggestTransaction == null)
            {
                Console.WriteLine("No transactions in blockchain.");
                return;
            }

            Console.WriteLine(
                $"Biggest transaction in network: " +
                $"Block #{blockNumber} | " +
                $"{biggestTransaction.From} -> {biggestTransaction.To} | " +
                $"Amount: {biggestTransaction.Amount}"
            );
        }
    }
}
