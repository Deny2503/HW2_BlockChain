using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class PeerFirewallStatus
    {
        public string IpAddress { get; set; } = string.Empty;
        public int Strikes { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
