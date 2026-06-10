using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class WalletKeystoreService
    {
        private readonly string folderPath;

        public WalletKeystoreService(string folderPath = "wallets")
        {
            this.folderPath = folderPath;
            Directory.CreateDirectory(folderPath);
        }

        public void SaveWallet(Wallet wallet, string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] iv = RandomNumberGenerator.GetBytes(16);
            byte[] privateKeyBytes = wallet.ExportPrivateKey();

            using Aes aes = Aes.Create();
            aes.Key = DeriveKey(password, salt);
            aes.IV = iv;

            byte[] encryptedPrivateKey;
            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            {
                encryptedPrivateKey = encryptor.TransformFinalBlock(privateKeyBytes, 0, privateKeyBytes.Length);
            }

            WalletFileDto dto = new WalletFileDto
            {
                OwnerName = wallet.OwnerName,
                PublicKey = wallet.PublicKey,
                Address = wallet.Address,
                Salt = Convert.ToBase64String(salt),
                IV = Convert.ToBase64String(iv),
                EncryptedPrivateKey = Convert.ToBase64String(encryptedPrivateKey)
            };

            string json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GetWalletPath(wallet.OwnerName), json);
        }

        public Wallet LoadWallet(string name, string password)
        {
            string path = GetWalletPath(name);
            if (!File.Exists(path))
            {
                throw new Exception("Гаманець не знайдено");
            }

            WalletFileDto? dto = JsonSerializer.Deserialize<WalletFileDto>(File.ReadAllText(path));
            if (dto == null)
            {
                throw new Exception("Файл гаманця пошкоджено");
            }

            try
            {
                byte[] salt = Convert.FromBase64String(dto.Salt);
                byte[] iv = Convert.FromBase64String(dto.IV);
                byte[] encryptedPrivateKey = Convert.FromBase64String(dto.EncryptedPrivateKey);

                using Aes aes = Aes.Create();
                aes.Key = DeriveKey(password, salt);
                aes.IV = iv;

                byte[] privateKeyBytes;
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    privateKeyBytes = decryptor.TransformFinalBlock(encryptedPrivateKey, 0, encryptedPrivateKey.Length);
                }

                return new Wallet(dto.OwnerName, privateKeyBytes, dto.PublicKey);
            }
            catch
            {
                throw new Exception("Невірний пароль");
            }
        }

        private static byte[] DeriveKey(string password, byte[] salt)
        {
            using Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32);
        }

        private string GetWalletPath(string name)
        {
            string safeName = string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(folderPath, $"wallet_{safeName}.json");
        }
    }

    public class WalletFileDto
    {
        public string OwnerName { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public string IV { get; set; } = string.Empty;
        public string EncryptedPrivateKey { get; set; } = string.Empty;
    }
}
