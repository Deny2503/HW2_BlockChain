using HW2_BlockChain;

Blockchain blockchain = new Blockchain();
BlockchainDisplay display = new BlockchainDisplay();

Wallet alice = new Wallet("Alice");
Wallet bob = new Wallet("Bob");
Wallet charlie = new Wallet("Charlie");

display.PrintWalletCard(alice);
display.PrintWalletCard(bob);

Console.WriteLine("\n===== MERKLE PROOF / SPV TEST =====");

Transaction secondTransaction = new Transaction(
    bob.Address,
    charlie.Address,
    amount: 4,
    senderPublicKey: bob.PublicKey,
    fee: 80
);
secondTransaction.Sign(bob);
blockchain.AddTransaction(secondTransaction);

Transaction thirdTransaction = new Transaction(
    charlie.Address,
    alice.Address,
    amount: 2,
    senderPublicKey: charlie.PublicKey,
    fee: 120
);
thirdTransaction.Sign(charlie);
blockchain.AddTransaction(thirdTransaction);

blockchain.AddBlock(blockchain.PendingTransactions.ToList());

Block lastBlock = blockchain.Chain[^1];
Transaction transactionForProof = lastBlock.Transactions[1];
string transactionHash = transactionForProof.Id;

List<MerkleProofItem> proof = MerkleAuditor.GetMerkleProof(
    lastBlock.MerkleTree,
    transactionHash
);

Console.WriteLine($"Selected transaction hash: {transactionHash}");
Console.WriteLine($"Merkle proof elements count: {proof.Count}");

for (int i = 0; i < proof.Count; i++)
{
    string side = proof[i].IsLeftNeighbor ? "left" : "right";
    Console.WriteLine($"Proof #{i + 1}: neighbor on {side}, hash = {proof[i].Hash}");
}

string originalMerkleRoot = lastBlock.MerkleRoot;
Console.WriteLine($"Original Merkle Root from block: {originalMerkleRoot}");

bool isVerified = MerkleAuditor.VerifyMerkleProof(
    transactionHash,
    proof,
    originalMerkleRoot
);

if (isVerified)
{
    Console.WriteLine("SPV verification success: transaction is proven without downloading all block transactions.");
}
else
{
    Console.WriteLine("SPV verification failed: transaction proof is invalid.");
}

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