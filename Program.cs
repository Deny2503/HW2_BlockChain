using HW2_BlockChain;

Block block = new Block(
    previousHash: "00000000000000000000000000000000",
    data: "Hello blockchain",
    difficulty: 4
);

Console.WriteLine("Mining started...");

Miner miner = new Miner();

MiningResult result = miner.Mine(block);

Console.WriteLine("Mining finished!");
Console.WriteLine($"Nonce: {result.Nonce}");
Console.WriteLine($"Hash:  {result.Hash}");