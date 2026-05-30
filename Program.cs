using HW2_BlockChain;

Blockchain blockchain = new Blockchain();
BlockchainDisplay display = new BlockchainDisplay();

Wallet alice = new Wallet("Alice");
Wallet bob = new Wallet("Bob");

display.PrintWalletCard(alice);
display.PrintWalletCard(bob);

Console.WriteLine("NORMAL TRANSACTION");

Transaction normalTransaction = new Transaction(
    alice.Address,
    bob.Address,
    25,
    alice.PublicKey
);

normalTransaction.Sign(alice);

blockchain.AddBlock(new List<Transaction>
{
    normalTransaction
});

Console.WriteLine();
Console.WriteLine("IDENTITY THEFT ATTACK");

Transaction fakeTransaction = new Transaction(
    alice.Address,
    bob.Address,
    100,
    bob.PublicKey
);

fakeTransaction.Sign(bob);

blockchain.AddBlock(new List<Transaction>
{
    fakeTransaction
});

Console.WriteLine();
Console.WriteLine("BROKEN SIGNATURE");

Transaction brokenTransaction = new Transaction(
    alice.Address,
    bob.Address,
    10,
    alice.PublicKey
);

brokenTransaction.Sign(alice);

brokenTransaction.Signature[0]++;

blockchain.AddBlock(new List<Transaction>
{
    brokenTransaction
});