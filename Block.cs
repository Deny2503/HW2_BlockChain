using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class Block
    {
        public int Index { get; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public long Timestamp { get; }
        public int Nonce { get; set; }
        public int Difficulty { get; }
        public List<Transaction> Transactions { get; }

        public Block(int index, string previousHash, List<Transaction> transactions, int difficulty)
        {
            Index = index;
            PreviousHash = previousHash;
            Transactions = transactions;
            Difficulty = difficulty;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Hash = "";
        }
    }
}
