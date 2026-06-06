using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class P2PMessage
    {
        public string Type { get; set; } = string.Empty;
        public JsonElement Payload { get; set; }
    }
}
