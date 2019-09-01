using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using iRaidDatamodel;
using iRaidDatamodel.Entities;
using iRaidPoRK.XmlDefinitions;
using iRaidPoRK.Helpers;

namespace iRaidPoRK
{
    public class PoRK
    {
        private static readonly User PorkUser = User.FindFirst(x => x.UserName == "PoRK");
        private bool Run;
        private System.Threading.Thread worker;
        private DateTime Update = DateTime.UtcNow.AddHours(-6);

        private static ConcurrentQueue<string> _UserLookup = new ConcurrentQueue<string>();
        private static ConcurrentQueue<long> _OrganizationLookup = new ConcurrentQueue<long>();

        public bool Start()
        {
            if(worker == null || !worker.IsAlive)
            {
                worker = new System.Threading.Thread(Worker);
                worker.IsBackground = true;
                worker.Start();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Stop()
        {
            if(worker != null && worker.IsAlive && Run)
            {
                Run = false;
            }
            else
            {
                Trace.WriteLine("Worker thread is not running.");
            }
        }

        private void Worker()
        {
            Trace.WriteLine("Starting PoRK worker thread.");

            Run = true;

            string name;
            long orgid;

            while (Run)
            {
                if ((DateTime.UtcNow - Update).TotalMinutes >= 15)
                {
                    UpdateCheck();
                }
                // Lets check if we have Organization queued before we eventually update single users.
                if (_OrganizationLookup.TryDequeue(out orgid))
                {
                    GetOrganization(orgid);
                    continue;
                }

                if(_UserLookup.TryDequeue(out name))
                {
                    GetCharacter(name);
                    continue;
                }

                System.Threading.Thread.Sleep(250);
            }
        }

        public static void CharacterEvent(string Name, long Id, bool Online)
        {
            Character character = Character.FindFirst(x => x.CharacterId == Id);

            if(character != null)
            {
                if (character.Nick != Name)
                {
                    string Old = character.Nick;
                    character.Nick = Name;
                    character.Save(PorkUser, string.Format("Nick updated to: {0} from: {1}", Name, Old));
                }
            }
            else
            {
                character = Character.FindFirst(x => x.Nick == Name);

                if (character == null)
                {
                    character = new Character();
                    character.Nick = Name;
                    character.CharacterId = Id;
                    character.Save(PorkUser);
                }
                else
                {
                    if (character.CharacterId == 0)
                    {
                        character.CharacterId = Id;
                        character.Save(PorkUser, string.Format("CharacterId set to: {0} from: {1}", Id, "0"));
                    }
                }
            }

            UserLookup(Name);
        }

        public static void UserLookup(string user)
        {
            _UserLookup.Enqueue(user);
        }

        public static void OrganizationLookup(long id)
        {
            if (id == 0)
                return;

            _OrganizationLookup.Enqueue(id);
        }

        private void UpdateCheck()
        {
            // find all characters that have not been updated past 24 hours.
            var Characters = Character.All().Where(x=>x.Updated <= DateTime.UtcNow.AddHours(-24)).ToList();

            var Orgs = (from t in Characters
                        where t.OrganizationId != 0
                        group t.OrganizationId by t.OrganizationId into g
                        select new { Id = g.Key, Count = g.Count() });

            foreach(var o in Orgs)
            {
                // Only if a org has more then 10 members that have not cynced in past 24 hours we will try to download Organization.
                if(o.Count > 10)
                {
                    OrganizationLookup(o.Id);
                }
            }

            int n = 0;
            // Users who are not organized.
            foreach(var c in Characters.Where(x=>x.OrganizationId == 0).OrderBy(x=>x.Updated).ToList())
            {
                n++;
                if(n > 50)
                {
                    break;
                }
                UserLookup(c.Nick);
            }

            // Users who are organized but have not been updated.
            if (n < 50)
            {
                foreach (var c in Characters.Where(x => x.OrganizationId != 0).OrderBy(x => x.Updated).ToList())
                {
                    n++;
                    if (n > 50)
                    {
                        break;
                    }
                    UserLookup(c.Nick);
                }
            }

            Update = DateTime.UtcNow;
        }

        private void GetOrganization(long id)
        {
            Trace.WriteLine(string.Format("Trying to download PoRK data for Organization: {0}", id));

            Organization organization = Organization.FindFirst(x => x.OrganizationId == id);

            if (organization != null && organization.LastUpdate().TotalHours <= 24)
            {
                Trace.WriteLine(string.Format("We have already downloaded PoRK data for this Organization less than 24 hours ago."));
                return;
            }

            try
            {
                var data = PoRKDownloadHelper.GetOrganization(id);

                if (organization == null)
                {
                    // Adding organization to database.
                    organization = new Organization(data.Name, data.Id);
                    organization.Save(PorkUser);
                }

                if (organization.Name != data.Name)
                {
                    string oldname = organization.Name;
                    organization.Name = data.Name;
                    organization.Save(PorkUser, string.Format("Organization name changed from {0} to {1}", oldname, data.Name));
                }

                var downloadedmembers = 0;

                foreach (var member in data.Members.Member)
                {
                    Character character = Character.FindFirst(x => x.Nick.ToLower() == member.Nickname.ToLower());

                    if (character != null)
                    {
                        // Update if we have not done so already.
                        if (character.LastUpdate().TotalHours <= 24)
                        {
                            continue;
                        }

                        bool Changed = false;
                        StringBuilder Changes = new StringBuilder();

                        #region Check if changes are detected.

                        if (character.Firstname != member.Firstname)
                        {
                            Changes.AppendFormat("Firstname updated from {0} to {1} ", character.Firstname, member.Firstname);
                            character.Firstname = member.Firstname;
                            Changed = true;
                        }

                        if (character.Lastname != member.Lastname)
                        {
                            Changes.AppendFormat("Lastname updated from {0} to {1} ", character.Lastname, member.Lastname);
                            character.Lastname = member.Lastname;
                            Changed = true;
                        }

                        if (character.Nick != member.Nickname)
                        {
                            Changes.AppendFormat("Nickname updated from {0} to {1} ", character.Nick, member.Nickname);
                            character.Nick = member.Nickname;
                            Changed = true;
                        }

                        if (character.Level != member.Level)
                        {
                            Changes.AppendFormat("Level updated from {0} to {1} ", character.Level, member.Level);
                            character.Level = member.Level;
                            Changed = true;
                        }

                        if (character.Gender != member.Gender)
                        {
                            Changes.AppendFormat("Gender updated from {0} to {1} ", character.Gender, member.Gender);
                            character.Gender = member.Gender;
                            Changed = true;
                        }

                        if (character.Breed != member.Breed)
                        {
                            Changes.AppendFormat("Breed updated from {0} to {1} ", character.Breed, member.Breed);
                            character.Gender = member.Gender;
                            Changed = true;
                        }

                        if (character.DefenderLevel != member.DefenderLevel)
                        {
                            Changes.AppendFormat("DefenderLevel updated from {0} to {1} ", character.DefenderLevel, member.DefenderLevel);
                            character.DefenderLevel = member.DefenderLevel;
                            Changed = true;
                        }

                        if (character.DefenderRank != member.DefenderRank)
                        {
                            Changes.AppendFormat("DefenderRank updated from {0} to {1} ", character.DefenderRank, member.DefenderRank);
                            character.DefenderRank = member.DefenderRank;
                            Changed = true;
                        }

                        if (character.Profession != member.Profession)
                        {
                            Changes.AppendFormat("Profession updated from {0} to {1} ", character.Profession, member.Profession);
                            character.Profession = member.Profession;
                            Changed = true;
                        }

                        if (character.Title != member.Title)
                        {
                            Changes.AppendFormat("Title updated from {0} to {1} ", character.Title, member.Title);
                            character.Title = member.Title;
                            Changed = true;
                        }

                        if (character.OrganizationRank != member.OrganizationRank)
                        {
                            Changes.AppendFormat("OrganizationRank updated from {0} to {1} ", character.OrganizationRank, member.OrganizationRank);
                            character.OrganizationRank = member.OrganizationRank;
                            Changed = true;
                        }

                        #endregion

                        // Only save is we actually made any changes.
                        if (Changed)
                        {
                            character.Updated = DateTime.UtcNow;
                            character.Save(PorkUser, Changes.ToString());
                            downloadedmembers++;
                        }
                        else
                        {
                            character.Updated = DateTime.UtcNow;
                            // No need to log that we saved user as no changes were detected.
                            character.Save();
                        }
                    }
                    else
                    {
                        // Add to database as character does not already exist.
                        character = new Character();
                        character.Firstname = member.Firstname;
                        character.Lastname = member.Lastname;
                        character.Nick = member.Nickname;
                        character.Level = member.Level;
                        character.OrganizationId = data.Id;
                        character.OrganizationRank = member.OrganizationRank;
                        character.Profession = member.Profession;
                        character.Gender = member.Gender;
                        character.Faction = data.Faction;
                        character.Breed = member.Breed;
                        character.DefenderLevel = member.DefenderLevel;
                        character.DefenderRank = member.DefenderRank;
                        character.Title = member.Title;
                        character.Updated = DateTime.UtcNow;
                        character.Save(PorkUser);
                        downloadedmembers++;
                    }
                }

                Trace.WriteLine(string.Format("Successfully downloaded PoRK data for {0}, downloaded information about {1} members.", data.Name, downloadedmembers));
            }
            catch
            {
                Trace.WriteLine(string.Format("Exception trying to download PoRK data for Organization."));

                if(organization != null)
                {
                    organization.Updated = DateTime.UtcNow;
                    organization.Save();
                }
            }
        }

        private void GetCharacter(string name)
        {
            Trace.WriteLine(string.Format("Trying to download PoRK data for Character: {0}", name));

            // lets find character record or create a new!
            Character character = Character.FindFirst(x => x.Nick.ToLower() == name.ToLower());

            if(character != null && character.LastUpdate().TotalHours <= 24)
            {
                Trace.WriteLine(string.Format("We have already downloaded PoRK data for this Character less than 24 hours ago."));

                OrganizationLookup(character.OrganizationId);

                return;
            }

            try
            {
                var data = PoRKDownloadHelper.GetCharacter(name);

                if(data == null || data.Name == null || data.Name.Nick == null || data.Name.Nick == "")
                {
                    Trace.WriteLine(string.Format("No official record could be found for Character: {0}", name));
                    // No record found.
                    return;
                }

                if (character == null)
                {
                    character = new Character() { Nick = name };
                }

                try
                {
                    character.Firstname = data.Name.Firstname;
                    character.Lastname = data.Name.Lastname;
                }
                catch
                {

                }

                character.Level = data.Stats.Level;
                character.Breed = data.Stats.Breed;
                character.Gender = data.Stats.Gender;
                character.Faction = data.Stats.Faction;
                character.Profession = data.Stats.Profession;
                character.Title = data.Stats.ProfessionTitle;
                character.DefenderLevel = data.Stats.DefenderLevel;
                character.DefenderRank = data.Stats.DefenderRank;

                try
                {
                    character.OrganizationId = data.Organization.OrganizationId;
                    character.OrganizationRank = data.Organization.Rank;
                }
                catch
                {
                    character.OrganizationId = 0;
                    character.OrganizationRank = string.Empty;
                }

                // Lets check if we have org data.
                if(character.CharacterId != 0)
                {
                    Organization organization = Organization.FindFirst(x => x.OrganizationId == character.CharacterId);

                    if(organization == null)
                    {
                        OrganizationLookup(character.OrganizationId);
                    }
                    else
                    {
                        if (organization.LastUpdate().TotalHours > 24)
                        {
                            OrganizationLookup(character.OrganizationId);
                        }
                    }
                }
                
                character.Updated = DateTime.UtcNow;
                character.Save(PorkUser);

                Trace.WriteLine(string.Format("PoRK successfully downloaded."));
            }
            catch
            {
                Trace.WriteLine(string.Format("Exception trying to download PoRK data for this user."));

                if (character != null)
                {
                    // could not find PoRK data for user, but lets prevent this record from being updated for another 24 hours.
                    character.Updated = DateTime.UtcNow;
                    character.Save();
                }
            }
        }
    }
}
