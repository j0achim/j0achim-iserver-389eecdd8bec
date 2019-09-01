//using System;
//using System.Collections.Generic;
//using System.Text;
using System.Xml.Serialization;

namespace iRaidPoRK.XmlDefinitions
{
    [XmlRoot("character")]
    public class FuncomCharacter
    {
        [XmlElement("name")]
        public CharacterName Name;

        [XmlElement("basic_stats")]
        public CharacerStats Stats;

        [XmlElement("organization_membership")]
        public CharacterOrganization Organization;
    }

    public class CharacterName
    {
        [XmlElement("firstname")]
        public string Firstname;
        [XmlElement("nick")]
        public string Nick;
        [XmlElement("lastname")]
        public string Lastname;
    }

    public class CharacerStats
    {
        [XmlElement("level")]
        public int Level;
        [XmlElement("breed")]
        public string Breed;
        [XmlElement("gender")]
        public string Gender;
        [XmlElement("faction")]
        public string Faction;
        [XmlElement("profession")]
        public string Profession;
        [XmlElement("profession_title")]
        public string ProfessionTitle;
        [XmlElement("defender_rank_id")]
        public int DefenderLevel;
        [XmlElement("defender_rank")]
        public string DefenderRank;
    }

    public class CharacterOrganization
    {
        [XmlElement("organization_id")]
        public long OrganizationId;
        [XmlElement("organization_name")]
        public string OrganizationName;
        [XmlElement("rank")]
        public string Rank;
    }
}
