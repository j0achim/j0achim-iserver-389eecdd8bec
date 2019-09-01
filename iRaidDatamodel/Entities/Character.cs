using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class Character : DomainLogic<Character>
    {
        public Character()
        {
            Gender = "Unknown";
            Breed = "Unknown";
            Title = "Unknown";
            Faction = "Unknown";
            Level = 1;
            DefenderLevel = 0;
            DefenderRank = "Unknown";
            Profession = "Unknown";
            OrganizationId = 0;
            OrganizationRank = "None";
            Updated = new DateTime(1970, 1, 1);
        }

        public long UserId { get; set; }
        public long CharacterId { get; set; }
        public string Nick { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Gender { get; set; }
        public string Breed { get; set; }
        public string Title { get; set; }
        public string Faction { get; set; }
        public int Level { get; set; }
        public int DefenderLevel { get; set; }
        public string DefenderRank { get; set; }
        public long OrganizationId { get; set; }
        public string OrganizationRank { get; set; }
        public string Profession { get; set; }
        public DateTime Updated { get; set; }

        public Organization Org
        {
            get
            {
                if(OrganizationId != 0)
                {
                    return iRaidDatamodel.Entities.Organization.FindFirst(x => x.Id == OrganizationId);
                }
                return null;
            }
        }

        public string MainRole
        {
            get
            {
                return GetRole(Profession, MainRoles());
            }
            set
            {
                
            }
        }

        public string SecondaryRole
        {
            get
            {
                return GetRole(Profession, SecondaryRoles());
            }
            set
            {

            }
        }

        public string GetRole(string profession, Dictionary<string, List<string>> roles)
        {
            foreach(var r in roles)
            {
                if(r.Value.Contains(profession))
                {
                    return r.Key;
                }
            }

            return "Unknown";
        }

        private Dictionary<string, List<string>> MainRoles()
        {
            Dictionary<string, List<string>> roles = new Dictionary<string, List<string>>();

            roles.Add("Tank", new List<string>()    { "Enforcer" });
            roles.Add("Damage", new List<string>()  { "Agent", "Shade", "Adventurer", "Meta-Physicist", "Soldier", "Fixer", "Keeper", "Nano-Technician", "Engineer", "Bureaucrat", "Martial-Artist", "Trader" });
            roles.Add("Healer", new List<string>()  { "Doctor" });

            return roles;
        }

        private Dictionary<string, List<string>> SecondaryRoles()
        {
            Dictionary<string, List<string>> roles = new Dictionary<string, List<string>>();

            roles.Add("Tank", new List<string>()    { "Soldier", "Adventurer" });
            roles.Add("Utility", new List<string>() { "Agent", "Fixer", "Keeper", "Nano-Technician", "Engineer", "Bureaucrat", "Trader", "Meta-Physicist" });
            roles.Add("Damage", new List<string>()  { "Doctor", "Enforcer" });
            roles.Add("Healer", new List<string>()  { "Martial-Artist" });
            roles.Add("None", new List<string>()    { "Shade" });

            return roles;
        }

        public TimeSpan LastUpdate()
        {
            return (DateTime.UtcNow - Updated);
        }

        public User User()
        {
            return Repository<User>.FindSingleBy(x => x.Id == this.UserId);
        }

        public Organization Organization()
        {
            return Repository<Organization>.FindFirstBy(x => x.OrganizationId == this.OrganizationId);
        }
    }
}
