using System.Collections.Generic;
using System.Xml.Serialization;

namespace iRaidPoRK.XmlDefinitions
{
    [XmlRoot("organization")]
    public class FuncomOrganization
    {
        [XmlElement("id")]
        public long Id;

        [XmlElement("name")]
        public string Name;

        [XmlElement("side")]
        public string Faction;

        [XmlElement("members")]
        public OrganizationMembers Members;
    }

    public class OrganizationMembers
    {
        [XmlElement("member")]
        public List<OrganizationMember> Member;
    }

    public class OrganizationMember
    {
        [XmlElement("firstname")]
        public string Firstname;
        [XmlElement("lastname")]
        public string Lastname;
        [XmlElement("nickname")]
        public string Nickname;
        [XmlElement("rank_name")]
        public string OrganizationRank;
        [XmlElement("rank")]
        public int OrganizationRankId;
        [XmlElement("level")]
        public int Level;
        [XmlElement("profession")]
        public string Profession;
        [XmlElement("profession_title")]
        public string Title;
        [XmlElement("gender")]
        public string Gender;
        [XmlElement("breed")]
        public string Breed;
        [XmlElement("defender_rank_id")]
        public int DefenderLevel;
        [XmlElement("defender_rank")]
        public string DefenderRank;
    }
}
