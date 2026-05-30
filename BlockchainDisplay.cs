using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class BlockchainDisplay
    {
        public void PrintWalletCard(Wallet wallet)
        {
            string shortPublicKey = wallet.PublicKey.Length > 20
                ? wallet.PublicKey.Substring(0, 20) + "..."
                : wallet.PublicKey;

            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        WALLET CARD                         ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ Owner:      {wallet.OwnerName,-46}║");
            Console.WriteLine($"║ Address:    {Cut(wallet.Address, 46),-46}║");
            Console.WriteLine($"║ Public Key: {shortPublicKey,-46}║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }

        private string Cut(string text, int maxLength)
        {
            if (text.Length <= maxLength)
            {
                return text;
            }

            return text.Substring(0, maxLength - 3) + "...";
        }
    }
}
