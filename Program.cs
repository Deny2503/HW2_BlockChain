using HW2_BlockChain;

Blockchain blockchain = new Blockchain();

blockchain.AddBlock(new List<Transaction>
{
    new Transaction("Alice", "Bob", 50),
    new Transaction("Bob", "Charlie", 15),
    new Transaction("Alice", "David", 20)
});

blockchain.AddBlock(new List<Transaction>
{
    new Transaction("Charlie", "Alice", 5),
    new Transaction("David", "Bob", 70),
    new Transaction("Bob", "Alice", 10)
});

blockchain.AddBlock(new List<Transaction>
{
    new Transaction("Alice", "Eve", 100),
    new Transaction("Eve", "Charlie", 25),
    new Transaction("Charlie", "David", 30)
});

blockchain.AddBlock(new List<Transaction>
{
    new Transaction("Bob", "Alice", 12),
    new Transaction("David", "Eve", 200),
    new Transaction("Eve", "Bob", 40)
});

Display display = new Display(blockchain);

display.PrintTransactionHistory("Alice");
display.PrintTransactionHistory("Batman");

display.PrintBiggestTransaction();