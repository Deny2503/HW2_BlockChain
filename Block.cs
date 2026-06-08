using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class Block
    {
        public int Index { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public long Timestamp { get; set; }
        public int Nonce { get; set; }
        public int Difficulty { get; set; }
        public List<Transaction> Transactions { get; set; }
        public List<List<string>> MerkleTree { get; set; }
        public string MerkleRoot { get; set; }

        public Block()
        {
            PreviousHash = string.Empty;
            Hash = string.Empty;
            Transactions = new List<Transaction>();
            MerkleTree = new List<List<string>>();
            MerkleRoot = string.Empty;
        }

        public Block(int index, string previousHash, List<Transaction> transactions, int difficulty)
        {
            Index = index;
            PreviousHash = previousHash;
            Transactions = transactions;
            MerkleTree = MerkleAuditor.BuildMerkleTree(transactions);
            MerkleRoot = MerkleTree[^1][0];
            Difficulty = difficulty;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Hash = "";
        }
    }
}
