using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    public class TokenTransaction : DomainLogic<TokenTransaction>
    {
        public TokenTransaction()
        {
            this.Time = DateTime.UtcNow;
        }

        public double Amount { get; set; }
        public double OldBalance { get; set; }
        public double NewBalance { get; set; }
        public DateTime Time { get; set; }
    }
}
