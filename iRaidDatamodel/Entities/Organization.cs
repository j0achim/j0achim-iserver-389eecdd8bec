using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class Organization : DomainLogic<Organization>
    {
        public Organization(string Name, long OrganizationId)
        {
            this.Name = Name;
            this.OrganizationId = OrganizationId;
            this.Updated = DateTime.UtcNow;
            this.Deleted = false;
        }

        public long OrganizationId { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Webpage { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }

        public TimeSpan LastUpdate()
        {
            return (Updated - DateTime.UtcNow);
        }

        public IEnumerable<Character> Members()
        {
            return Repository<Character>.FindBy(x => x.OrganizationId == this.Id);
        }
    }
}
