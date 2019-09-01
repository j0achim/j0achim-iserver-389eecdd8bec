using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class Wiki : DomainLogic<Wiki>
    {
        public Wiki(string Title, string Body, long CommunityId = 0)
        {
            this.CommunityId = CommunityId;
            this.Title = Title;
            this.Body = Body;
            this.Index = false;
            this.Deleted = false;
            this.AuthRequired = false;
            this.Protected = false;
            this.Version = 0;
        }

        public Wiki(Community community, string Title, string Body)
        {
            this.CommunityId = community.Id;
            this.Title = Title;
            this.Body = Body;
            this.Index = false;
            this.Deleted = false;
            this.AuthRequired = false;
            this.Protected = false;
        }

        public long CommunityId { get; private set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool Index { get; set; }
        public bool Deleted { get; set; }
        public bool AuthRequired { get; set; }
        public bool Protected { get; set; }
        public int Version { get; set; }

        public void SaveWiki(User user)
        {
            this.Version++;

            // Same wiki page.
            this.Save(user);
        }
    }
}
