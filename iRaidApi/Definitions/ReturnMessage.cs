using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidApi.Definitions
{
    public class ReturnMessage
    {
        public ReturnMessage()
        {
            this.time = DateTime.UtcNow;
        }

        public string action { get; set; }
        public bool success { get; set; }
        public string reply { get; set; }
        public DateTime time { get; set; }
        public string executiontime { get; set; }
    }
}
