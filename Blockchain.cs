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
        private const int Difficulty = 4;

        public Blockchain()
        {
            Chain = new List<Block>();
            CreateGenesisBlock();
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
            Block previousBlock = Chain[^1];

            Block newBlock = new Block(
                index: Chain.Count,
                previousHash: previousBlock.Hash,
                transactions: transactions,
                difficulty: Difficulty
            );

            Miner miner = new Miner();

            MiningResult result = miner.Mine(newBlock);

            newBlock.Nonce = result.Nonce;
            newBlock.Hash = result.Hash;

            Chain.Add(newBlock);
        }
    }
}
