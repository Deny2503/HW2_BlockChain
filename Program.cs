using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using HW2_BlockChain;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

Blockchain blockchain = new Blockchain();
BlockchainExplorerService explorer = new BlockchainExplorerService(blockchain);
WalletKeystoreService keystore = new WalletKeystoreService();

Console.WriteLine("====================================");
Console.WriteLine("   EXAM BLOCKCHAIN PRODUCT v1.0");
Console.WriteLine("====================================");

Wallet currentWallet = StartWalletFlow(keystore);
Console.WriteLine($"\nАктивний гаманець: {currentWallet.OwnerName}");
Console.WriteLine($"Адреса: {currentWallet.Address}");

while (true)
{
    Console.WriteLine("\n========== МЕНЮ ==========");
    Console.WriteLine("1. Відправити токен / MAIN");
    Console.WriteLine("2. Випустити власний токен (Mint)");
    Console.WriteLine("3. Переглянути історію гаманця");
    Console.WriteLine("4. Знайти блок за TxID");
    Console.WriteLine("5. Показати всі баланси");
    Console.WriteLine("6. Майнити pending-транзакції");
    Console.WriteLine("7. Знайти транзакцію за TxID");
    Console.WriteLine("8. Показати зароблені комісії майнера");
    Console.WriteLine("0. Вийти");
    Console.Write("Ваш вибір: ");

    string? choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
                SendTransaction(blockchain, currentWallet);
                break;
            case "2":
                MintToken(blockchain, currentWallet);
                break;
            case "3":
                ShowHistory(explorer, currentWallet.Address);
                break;
            case "4":
                FindBlock(explorer);
                break;
            case "5":
                ShowBalances(blockchain, currentWallet.Address);
                break;
            case "6":
                blockchain.MinePendingTransactions(currentWallet.Address);
                break;
            case "7":
                FindTransaction(explorer);
                break;
            case "8":
                Console.WriteLine($"Комісії майнера: {explorer.GetTotalFeesEarned(currentWallet.Address):F2} MAIN");
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Невідомий пункт меню.");
                break;
        }
    }
    catch (Exception exception)
    {
        Console.WriteLine($"Помилка: {exception.Message}");
    }
}

static Wallet StartWalletFlow(WalletKeystoreService keystore)
{
    while (true)
    {
        Console.WriteLine("\n1. Створити новий гаманець");
        Console.WriteLine("2. Завантажити існуючий гаманець");
        Console.Write("Ваш вибір: ");
        string? choice = Console.ReadLine();

        Console.Write("Ім'я гаманця: ");
        string name = Console.ReadLine() ?? "User";

        Console.Write("Пароль: ");
        string password = ReadPassword();

        if (choice == "1")
        {
            Wallet wallet = new Wallet(name);
            keystore.SaveWallet(wallet, password);
            Console.WriteLine("Гаманець створено і збережено.");
            return wallet;
        }

        if (choice == "2")
        {
            Wallet wallet = keystore.LoadWallet(name, password);
            Console.WriteLine("Гаманець завантажено.");
            return wallet;
        }

        Console.WriteLine("Оберіть 1 або 2.");
    }
}

static string ReadPassword()
{
    string password = string.Empty;
    ConsoleKeyInfo key;

    while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
    {
        if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password = password[..^1];
            Console.Write("\b \b");
        }
        else if (!char.IsControl(key.KeyChar))
        {
            password += key.KeyChar;
            Console.Write("*");
        }
    }

    Console.WriteLine();
    return password;
}

static void SendTransaction(Blockchain blockchain, Wallet wallet)
{
    Console.Write("Адреса отримувача: ");
    string to = Console.ReadLine() ?? string.Empty;

    Console.Write("Символ токена (MAIN / ITSTEP_COIN): ");
    string tokenSymbol = Console.ReadLine() ?? "MAIN";

    Console.Write("Сума: ");
    decimal amount = decimal.Parse(Console.ReadLine() ?? "0");

    decimal suggestedFee = Math.Ceiling(200 * blockchain.GetCurrentNetworkFee());
    Console.Write($"Комісія MAIN, рекомендовано >= {suggestedFee:F2}: ");
    decimal fee = decimal.Parse(Console.ReadLine() ?? suggestedFee.ToString());

    Transaction transaction = new Transaction(wallet.Address, to, amount, wallet.PublicKey, fee, tokenSymbol);
    transaction.Sign(wallet);

    blockchain.AddTransaction(transaction);
    Console.WriteLine($"Транзакцію додано в mempool. TxID: {transaction.Id}");
}

static void MintToken(Blockchain blockchain, Wallet wallet)
{
    Console.Write("Символ нового токена: ");
    string tokenSymbol = Console.ReadLine() ?? "TOKEN";

    Console.Write("Кількість: ");
    decimal amount = decimal.Parse(Console.ReadLine() ?? "0");

    Transaction mintTransaction = new Transaction("MINT", wallet.Address, amount, "", 0, tokenSymbol);
    blockchain.AddTransaction(mintTransaction);

    Console.WriteLine($"Mint-транзакцію додано в mempool. TxID: {mintTransaction.Id}");
}

static void ShowHistory(BlockchainExplorerService explorer, string address)
{
    List<Transaction> history = explorer.GetTransactionHistory(address);

    if (history.Count == 0)
    {
        Console.WriteLine("Історія порожня.");
        return;
    }

    foreach (Transaction transaction in history)
    {
        string direction = transaction.To == address ? "IN " : "OUT";
        Console.WriteLine($"{direction} | {transaction.TokenSymbol} | {transaction.Amount:F2} | Fee {transaction.Fee:F2} | TxID {transaction.Id}");
    }
}

static void FindBlock(BlockchainExplorerService explorer)
{
    Console.Write("TxID: ");
    string txId = Console.ReadLine() ?? string.Empty;

    Block? block = explorer.FindBlockByTransactionId(txId);
    if (block == null)
    {
        Console.WriteLine("Блок не знайдено. Можливо, транзакція ще в mempool.");
        return;
    }

    Console.WriteLine($"Знайдено блок #{block.Index}");
    Console.WriteLine($"Hash: {block.Hash}");
    Console.WriteLine($"Miner: {block.MinerAddress}");
}

static void FindTransaction(BlockchainExplorerService explorer)
{
    Console.Write("TxID: ");
    string txId = Console.ReadLine() ?? string.Empty;

    Transaction? transaction = explorer.FindTransactionById(txId);
    if (transaction == null)
    {
        Console.WriteLine("Транзакцію не знайдено.");
        return;
    }

    Console.WriteLine($"From: {transaction.From}");
    Console.WriteLine($"To: {transaction.To}");
    Console.WriteLine($"Token: {transaction.TokenSymbol}");
    Console.WriteLine($"Amount: {transaction.Amount:F2}");
    Console.WriteLine($"Fee: {transaction.Fee:F2}");
}

static void ShowBalances(Blockchain blockchain, string address)
{
    foreach (string token in blockchain.GetKnownTokenSymbols())
    {
        decimal balance = blockchain.GetPendingBalance(address, token);
        if (balance != 0 || token == "MAIN")
        {
            Console.WriteLine($"{token}: {balance:F2}");
        }
    }
}
