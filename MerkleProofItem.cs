using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class MerkleProofItem
    {
        public string Hash { get; }
        public bool IsLeftNeighbor { get; }

        public MerkleProofItem(string hash, bool isLeftNeighbor)
        {
            Hash = hash;
            IsLeftNeighbor = isLeftNeighbor;
        }
    }
}
