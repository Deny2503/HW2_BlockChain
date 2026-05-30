using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class Transaction
    {
        public string From { get; }
        public string To { get; }
        public decimal Amount { get; }
        public string SenderPublicKey { get; }
        public byte[] Signature { get; set; }

        public Transaction(string from, string to, decimal amount, string senderPublicKey)
        {
            From = from;
            To = to;
            Amount = amount;
            SenderPublicKey = senderPublicKey;
            Signature = Array.Empty<byte>();
        }

        public string GetDataToSign()
        {
            return From + To + Amount;
        }

        public void Sign(Wallet wallet)
        {
            Signature = wallet.Sign(GetDataToSign());
        }

        public bool IsValid()
        {
            string realAddress = Wallet.GenerateAddress(SenderPublicKey);

            if (realAddress != From)
            {
                Console.WriteLine("SECURITY ERROR: Public key does not match sender address.");
                return false;
            }

            bool signatureIsValid = VerifySignature();

            if (!signatureIsValid)
            {
                Console.WriteLine("SECURITY ERROR: Signature is broken or invalid.");
                return false;
            }

            return true;
        }

        private bool VerifySignature()
        {
            try
            {
                byte[] publicKeyBytes = Convert.FromBase64String(SenderPublicKey);
                byte[] dataBytes = Encoding.UTF8.GetBytes(GetDataToSign());

                using ECDsa ecdsa = ECDsa.Create();
                ecdsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

                return ecdsa.VerifyData(
                    dataBytes,
                    Signature,
                    HashAlgorithmName.SHA256
                );
            }
            catch
            {
                return false;
            }
        }
    }
}
