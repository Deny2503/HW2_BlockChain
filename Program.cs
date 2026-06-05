using HW2_BlockChain;

Blockchain blockchain = new Blockchain();
Wallet miner = new Wallet("Miner");
Wallet helper = new Wallet("Helper");

Console.WriteLine("===== COINBASE MATURITY TEST =====");
Console.WriteLine($"Coinbase maturity rule: {blockchain.CoinbaseMaturity} confirmations");

Transaction minerReward = new Transaction(
    from: "COINBASE",
    to: miner.Address,
    amount: 50,
    senderPublicKey: "",
    fee: 0
);

blockchain.AddBlock(new List<Transaction> { minerReward });
PrintCoinbaseStatus(blockchain, miner.Address, minerReward.Id, "After mining reward block");

for (int i = 1; i <= blockchain.CoinbaseMaturity; i++)
{
    Transaction fillerReward = new Transaction(
        from: "COINBASE",
        to: helper.Address,
        amount: 1,
        senderPublicKey: "",
        fee: 0
    );

    blockchain.AddBlock(new List<Transaction> { fillerReward });
    PrintCoinbaseStatus(blockchain, miner.Address, minerReward.Id, $"After {i} extra block(s)");
}

Console.WriteLine("\nResult: miner reward appears in balance only after enough confirmations.");

static void PrintCoinbaseStatus(
    Blockchain blockchain,
    string minerAddress,
    string rewardTransactionId,
    string label)
{
    Block? rewardBlock = blockchain.Chain.FirstOrDefault(block =>
        block.Transactions.Any(transaction => transaction.Id == rewardTransactionId));

    if (rewardBlock == null)
    {
        Console.WriteLine($"\n{label}");
        Console.WriteLine("Reward block was not found in the chain.");
        return;
    }

    int confirmations = blockchain.Chain.Count - rewardBlock.Index;
    decimal balance = blockchain.GetBalance(minerAddress);

    Console.WriteLine($"\n{label}");
    Console.WriteLine($"Reward block index: {rewardBlock.Index}");
    Console.WriteLine($"Confirmations: {confirmations}");
    Console.WriteLine($"Miner balance: {balance}");
}
