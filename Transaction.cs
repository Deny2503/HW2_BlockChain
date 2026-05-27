using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class Transaction
    {
        public string From { get; }
        public string To { get; }
        public decimal Amount { get; }

        public Transaction(string from, string to, decimal amount)
        {
            From = from;
            To = to;
            Amount = amount;
        }
    }
}
