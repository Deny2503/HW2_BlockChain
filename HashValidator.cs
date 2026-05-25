using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public static class HashValidator
    {
        public static bool IsValid(ReadOnlySpan<byte> hashBytes, int difficulty)
        {
            int fullZeroBytes = difficulty / 2;
            bool needHalfByte = difficulty % 2 == 1;

            for (int i = 0; i < fullZeroBytes; i++)
            {
                if (hashBytes[i] != 0)
                {
                    return false;
                }
            }

            if (needHalfByte)
            {
                return (hashBytes[fullZeroBytes] & 0xF0) == 0;
            }

            return true;
        }
    }
}
