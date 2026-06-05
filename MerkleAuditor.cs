using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public static class MerkleAuditor
    {
        public static string CalculateHash(string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] hashBytes = SHA256.HashData(dataBytes);

            return Convert.ToHexString(hashBytes).ToLower();
        }

        public static List<List<string>> BuildMerkleTree(List<Transaction> transactions)
        {
            if (transactions.Count == 0)
            {
                return new List<List<string>>
                {
                    new List<string> { CalculateHash(string.Empty) }
                };
            }

            List<List<string>> tree = new();
            List<string> currentLevel = transactions
                .Select(transaction => transaction.Id)
                .ToList();

            tree.Add(currentLevel);

            while (currentLevel.Count > 1)
            {
                List<string> nextLevel = new();

                for (int i = 0; i < currentLevel.Count; i += 2)
                {
                    string left = currentLevel[i];
                    string right = i + 1 < currentLevel.Count
                        ? currentLevel[i + 1]
                        : left;

                    nextLevel.Add(CalculateHash(left + right));
                }

                currentLevel = nextLevel;
                tree.Add(currentLevel);
            }

            return tree;
        }

        public static string GetMerkleRoot(List<Transaction> transactions)
        {
            List<List<string>> tree = BuildMerkleTree(transactions);

            return tree[^1][0];
        }

        public static List<MerkleProofItem> GetMerkleProof(
            List<List<string>> tree,
            string transactionHash)
        {
            if (tree.Count == 0)
            {
                throw new Exception("Дерево Меркла порожнє");
            }

            int currentIndex = tree[0].IndexOf(transactionHash);

            if (currentIndex == -1)
            {
                throw new Exception("Транзакцію не знайдено у дереві Меркла");
            }

            List<MerkleProofItem> proof = new();

            for (int levelIndex = 0; levelIndex < tree.Count - 1; levelIndex++)
            {
                List<string> level = tree[levelIndex];
                bool currentIsRightChild = currentIndex % 2 == 1;

                int neighborIndex = currentIsRightChild
                    ? currentIndex - 1
                    : currentIndex + 1;

                if (neighborIndex >= level.Count)
                {
                    neighborIndex = currentIndex;
                }

                proof.Add(new MerkleProofItem(
                    level[neighborIndex],
                    isLeftNeighbor: currentIsRightChild
                ));

                currentIndex /= 2;
            }

            return proof;
        }

        public static bool VerifyMerkleProof(
            string transactionHash,
            List<MerkleProofItem> proof,
            string expectedMerkleRoot)
        {
            string currentHash = transactionHash;

            foreach (MerkleProofItem proofItem in proof)
            {
                currentHash = proofItem.IsLeftNeighbor
                    ? CalculateHash(proofItem.Hash + currentHash)
                    : CalculateHash(currentHash + proofItem.Hash);
            }

            return currentHash == expectedMerkleRoot;
        }
    }
}
