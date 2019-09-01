using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidApi.Definitions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Command : Attribute
    {
        public bool Confidential { get; set; }
        public string Name { get; private set; }
        public bool AuthRequired { get; set; }
        public string Description { get; set; }
        public string Help { get; set; }
        public string Category { get; set; }

        public Command(bool Confidential = false, bool AuthRequired = false, string Category = "Uncategorized")
        {
            this.Name = string.Empty;
            this.Confidential = Confidential;
            this.AuthRequired = AuthRequired;
            this.Category = Category;
        }
    }
}
