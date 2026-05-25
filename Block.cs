using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class Block
    {
        public string PreviousHash { get; }
        public string Data { get; }
        public long Timestamp { get; }
        public int Difficulty { get; }

        public Block(string previousHash, string data, int difficulty)
        {
            PreviousHash = previousHash;
            Data = data;
            Difficulty = difficulty;

            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
