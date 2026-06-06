using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using HW2_BlockChain;

Blockchain blockchain = new Blockchain();
P2PNetworkService p2pNetworkService = new P2PNetworkService(blockchain, port: 5000);
Task? serverTask = null;

while (true)
{
    Console.WriteLine();
    Console.WriteLine("1. Start P2P node");
    Console.WriteLine("2. Send invalid JSON test packet");
    Console.WriteLine("3. Send fake block test packet");
    Console.WriteLine("4. Show Firewall Blacklist");
    Console.WriteLine("0. Exit");
    Console.Write("Choose option: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            if (serverTask == null)
            {
                serverTask = Task.Run(() => p2pNetworkService.StartAsync());
                await Task.Delay(300);
            }
            else
            {
                Console.WriteLine("P2P node is already started.");
            }
            break;

        case "2":
            await SendRawPacketAsync("THIS_IS_NOT_JSON");
            break;

        case "3":
            await SendFakeBlockAsync(blockchain);
            break;

        case "4":
            p2pNetworkService.ShowFirewallBlacklist();
            break;

        case "0":
            p2pNetworkService.Stop();
            return;

        default:
            Console.WriteLine("Unknown menu option.");
            break;
    }
}

static async Task SendRawPacketAsync(string message)
{
    try
    {
        using TcpClient client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", 5000);

        await using NetworkStream stream = client.GetStream();
        byte[] bytes = Encoding.UTF8.GetBytes(message + Environment.NewLine);
        await stream.WriteAsync(bytes);

        Console.WriteLine("Test packet sent.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unable to send packet. Start the P2P node first. Error: {ex.Message}");
    }
}

static async Task SendFakeBlockAsync(Blockchain blockchain)
{
    Block previousBlock = blockchain.Chain[^1];

    Block fakeBlock = new Block(
        index: previousBlock.Index + 1,
        previousHash: previousBlock.Hash,
        transactions: new List<Transaction>
        {
            new Transaction("COINBASE", "fake-miner", 50, "", 0)
        },
        difficulty: 4
    );

    fakeBlock.Nonce = 123;
    fakeBlock.Hash = "fake_hash_without_valid_proof_of_work";

    string packet = JsonSerializer.Serialize(new
    {
        Type = "BLOCK",
        Payload = fakeBlock
    });

    await SendRawPacketAsync(packet);
}
