using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class Miner
    {
        private string GetTransactionsText(List<Transaction> transactions)
        {
            StringBuilder builder = new StringBuilder();

            foreach (Transaction transaction in transactions)
            {
                builder.Append(transaction.From);
                builder.Append(transaction.To);
                builder.Append(transaction.Amount);
                builder.Append(transaction.Fee);
                builder.Append(transaction.TokenSymbol);
                builder.Append(transaction.Timestamp);
            }

            return builder.ToString();
        }

        public MiningResult Mine(Block block)
        {
            byte[] prefixBytes = Encoding.UTF8.GetBytes(
                block.PreviousHash + GetTransactionsText(block.Transactions) + block.Timestamp
            );

            int threadCount = Environment.ProcessorCount;

            using CancellationTokenSource cancellationTokenSource = new();

            object lockObject = new object();

            int foundNonce = -1;
            byte[]? foundHashBytes = null;

            try
            {
                Parallel.For(0, threadCount, new ParallelOptions
                {
                    CancellationToken = cancellationTokenSource.Token
                },
                threadIndex =>
                {
                    byte[] buffer = new byte[prefixBytes.Length + 4];

                    Array.Copy(prefixBytes, buffer, prefixBytes.Length);

                    Span<byte> hashBytes = stackalloc byte[32];

                    int currentNonce = threadIndex;

                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        BinaryPrimitives.WriteInt32LittleEndian(
                            buffer.AsSpan(prefixBytes.Length, 4),
                            currentNonce
                        );

                        SHA256.HashData(buffer, hashBytes);

                        if (HashValidator.IsValid(hashBytes, block.Difficulty))
                        {
                            lock (lockObject)
                            {
                                if (foundNonce == -1)
                                {
                                    foundNonce = currentNonce;
                                    foundHashBytes = hashBytes.ToArray();

                                    cancellationTokenSource.Cancel();
                                }
                            }

                            break;
                        }

                        currentNonce += threadCount;
                    }
                });
            }
            catch (OperationCanceledException)
            {
            }

            if (foundHashBytes == null)
            {
                throw new Exception("Hash was not found.");
            }

            string hash = HashConverter.ToHex(foundHashBytes);

            return new MiningResult(foundNonce, hash);
        }
    }
}
