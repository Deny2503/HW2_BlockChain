using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public static class HashConverter
    {
        public static string ToHex(byte[] bytes)
        {
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
