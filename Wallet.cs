using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class Wallet
    {
        private readonly ECDsa privateKey;

        public string OwnerName { get; }
        public string PublicKey { get; }
        public string Address { get; }

        public Wallet(string ownerName)
        {
            OwnerName = ownerName;

            privateKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);

            byte[] publicKeyBytes = privateKey.ExportSubjectPublicKeyInfo();

            PublicKey = Convert.ToBase64String(publicKeyBytes);
            Address = GenerateAddress(PublicKey);
        }

        public byte[] Sign(string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            return privateKey.SignData(
                dataBytes,
                HashAlgorithmName.SHA256
            );
        }

        public static string GenerateAddress(string publicKey)
        {
            byte[] publicKeyBytes = Encoding.UTF8.GetBytes(publicKey);
            byte[] hashBytes = SHA256.HashData(publicKeyBytes);

            return Convert.ToHexString(hashBytes).ToLower();
        }
    }
}
