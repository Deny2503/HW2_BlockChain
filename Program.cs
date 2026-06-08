using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using HW2_BlockChain;

Console.WriteLine("===== CONSENSUS SECURITY TEST =====");
Console.WriteLine("Time Warp Protection + Max Reorg Depth");
Console.WriteLine();

Blockchain nodeA = new Blockchain();
Blockchain nodeB = new Blockchain();

nodeA.MaxReorgDepth = 5;
nodeB.MaxReorgDepth = 5;

Console.WriteLine($"MaxReorgDepth: {nodeA.MaxReorgDepth}");
Console.WriteLine("Shared genesis block hash: " + nodeA.Chain[0].Hash);
Console.WriteLine();

Console.WriteLine("--- Part 1: Time Warp Protection ---");
Block previousBlock = nodeA.Chain[^1];

Block futureBlock = new Block(
    index: previousBlock.Index + 1,
    previousHash: previousBlock.Hash,
    transactions: new List<Transaction>
    {
        new Transaction("COINBASE", "time-warp-attacker", 50, "", 0)
    },
    difficulty: 4
);

futureBlock.Timestamp = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds();
MiningResult futureMiningResult = new Miner().Mine(futureBlock);
futureBlock.Nonce = futureMiningResult.Nonce;
futureBlock.Hash = futureMiningResult.Hash;

bool futureBlockAccepted = nodeA.TryAddBlockFromPeer(futureBlock);
Console.WriteLine($"Future timestamp block accepted: {futureBlockAccepted}");
Console.WriteLine();

Console.WriteLine("--- Part 2: 51% Attack / Deep Reorg Simulation ---");
Console.WriteLine("NodeA honest network mines blocks #1..#6");
MineBlocks(nodeA, "honest-miner", 6);

Console.WriteLine();
Console.WriteLine("NodeB attacker secretly mines longer chain #1..#8 from genesis");
MineBlocks(nodeB, "attacker-miner", 8);

Console.WriteLine();
Console.WriteLine($"NodeA chain height: {nodeA.Chain[^1].Index}");
Console.WriteLine($"NodeB chain height: {nodeB.Chain[^1].Index}");
Console.WriteLine($"NodeB chain is longer: {nodeB.Chain.Count > nodeA.Chain.Count}");
Console.WriteLine();

Console.WriteLine("Attacker sends longer chain to NodeA...");
bool reorgAccepted = nodeA.ResolveConflicts(nodeB.Chain);

Console.WriteLine();
Console.WriteLine($"Reorg accepted: {reorgAccepted}");
Console.WriteLine($"NodeA final height: {nodeA.Chain[^1].Index}");
Console.WriteLine(
    reorgAccepted
        ? "Result: protection failed. Hacker chain replaced local history."
        : "Result: protection worked. Deep reorg attack was rejected."
);

static void MineBlocks(Blockchain blockchain, string minerAddress, int count)
{
    for (int i = 0; i < count; i++)
    {
        blockchain.AddBlock(new List<Transaction>
        {
            new Transaction("COINBASE", minerAddress, 50, "", 0)
        });
    }
}
