using HW2_BlockChain;

Blockchain blockchain = new Blockchain();
BlockchainDisplay display = new BlockchainDisplay();

Wallet alice = new Wallet("Alice");
Wallet bob = new Wallet("Bob");

display.PrintWalletCard(alice);
display.PrintWalletCard(bob);

Console.WriteLine("===== RBF TEST =====");

Transaction oldTransaction = new Transaction(
    alice.Address,
    bob.Address,
    amount: 10,
    senderPublicKey: alice.PublicKey,
    fee: 25
);

oldTransaction.Sign(alice);
blockchain.AddTransaction(oldTransaction);

Console.WriteLine("\nMempool BEFORE RBF:");
PrintMempool(blockchain);

Transaction newTransaction = new Transaction(
    alice.Address,
    bob.Address,
    amount: 10,
    senderPublicKey: alice.PublicKey,
    fee: 50
);

newTransaction.ReplacesTxId = oldTransaction.Id;
newTransaction.Sign(alice);

blockchain.AddTransaction(newTransaction);

Console.WriteLine("\nMempool AFTER RBF:");
PrintMempool(blockchain);

static void PrintMempool(Blockchain blockchain)
{
    Console.WriteLine($"Current network fee: {blockchain.GetCurrentNetworkFee():F2} per byte");
    Console.WriteLine($"Transactions in mempool: {blockchain.PendingTransactions.Count}");

    foreach (Transaction transaction in blockchain.PendingTransactions)
    {
        Console.WriteLine(
            $"ID: {transaction.Id[..12]}... | " +
            $"From: {transaction.From[..12]}... | " +
            $"To: {transaction.To[..12]}... | " +
            $"Amount: {transaction.Amount} | " +
            $"Fee: {transaction.Fee} | " +
            $"Size: {transaction.Size} bytes | " +
            $"Replaces: {(transaction.ReplacesTxId == null ? "none" : transaction.ReplacesTxId[..12] + "...")}"
        );
    }
}