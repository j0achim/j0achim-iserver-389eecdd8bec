using System;
using System.Xml.Linq;
using System.Xml.Serialization;
using iRaidPoRK.XmlDefinitions;

namespace iRaidPoRK.Helpers
{
    public class PoRKDownloadHelper
    {
        public static FuncomCharacter GetCharacter(string name)
        {
            var document = GetCharacterXML(name);

            if (document != null)
            {
                FuncomCharacter character = SerializationUtil.Deserialize<FuncomCharacter>(document);

                return character;
            }
            else
            {
                throw new Exception(string.Format("Could not get xml document for User: {0}", name));
            }
        }

        public static FuncomOrganization GetOrganization(long id)
        {
            var document = GetOrganizationXML(id);

            if (document != null)
            {
                FuncomOrganization organization = SerializationUtil.Deserialize<FuncomOrganization>(document);

                return organization;
            }
            else
            {
                throw new Exception(string.Format("Could not get xml document for OrganizationId: {0}", id));
            }
        }

        private static XDocument GetCharacterXML(string name)
        {
            return GetDocument(string.Format(@"http://people.anarchy-online.com/character/bio/d/5/name/{0}/bio.xml", name));
        }

        private static XDocument GetOrganizationXML(long id)
        {
            return GetDocument(string.Format(@"http://people.anarchy-online.com/org/stats/d/5/name/{0}/basicstats.xml", id));
        }

        private static XDocument GetDocument(string documentpath)
        {
            try
            {
                return XDocument.Load(documentpath);
            }
            catch
            {
                return null;
            }
        }
    }
}
