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
        public decimal Fee { get; }
        public string SenderPublicKey { get; }
        public string TokenSymbol { get; set; } = "MAIN";
        public byte[] Signature { get; set; }
        public string? ReplacesTxId { get; set; } = null;
        public long Timestamp { get; set; }

        public string Id
        {
            get
            {
                string signatureText = Convert.ToBase64String(Signature);
                string data = GetDataToSign() + signatureText + ReplacesTxId;
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] hashBytes = SHA256.HashData(dataBytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }

        public int Size => Encoding.UTF8.GetByteCount(GetDataToSign())
            + Encoding.UTF8.GetByteCount(SenderPublicKey)
            + Signature.Length
            + Encoding.UTF8.GetByteCount(ReplacesTxId ?? "");

        public Transaction(string from, string to, decimal amount, string senderPublicKey, decimal fee = 0, string tokenSymbol = "MAIN")
        {
            From = from;
            To = to;
            Amount = amount;
            Fee = fee;
            SenderPublicKey = senderPublicKey;
            TokenSymbol = string.IsNullOrWhiteSpace(tokenSymbol) ? "MAIN" : tokenSymbol.Trim().ToUpperInvariant();
            Signature = Array.Empty<byte>();
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public string GetDataToSign()
        {
            return From + To + Amount + Fee + TokenSymbol + Timestamp + ReplacesTxId;
        }

        public void Sign(Wallet wallet)
        {
            Signature = wallet.Sign(GetDataToSign());
        }

        public bool IsSystemTransaction()
        {
            return From == "COINBASE" || From == "MINT";
        }

        public bool IsValid()
        {
            if (Amount <= 0 || Fee < 0 || string.IsNullOrWhiteSpace(To))
            {
                return false;
            }

            if (IsSystemTransaction())
            {
                return true;
            }

            string realAddress = Wallet.GenerateAddress(SenderPublicKey);
            if (realAddress != From)
            {
                Console.WriteLine("SECURITY ERROR: Public key does not match sender address.");
                return false;
            }

            if (!VerifySignature())
            {
                Console.WriteLine("SECURITY ERROR: Signature is broken or invalid.");
                return false;
            }

            return true;
        }

        public bool VerifySignature()
        {
            try
            {
                byte[] publicKeyBytes = Convert.FromBase64String(SenderPublicKey);
                byte[] dataBytes = Encoding.UTF8.GetBytes(GetDataToSign());

                using ECDsa ecdsa = ECDsa.Create();
                ecdsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

                return ecdsa.VerifyData(dataBytes, Signature, HashAlgorithmName.SHA256);
            }
            catch
            {
                return false;
            }
        }
    }
}
