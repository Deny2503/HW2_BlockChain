using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class MiningResult
    {
        public int Nonce { get; }
        public string Hash { get; }

        public MiningResult(int nonce, string hash)
        {
            Nonce = nonce;
            Hash = hash;
        }
    }
}
