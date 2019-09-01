using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class Member : DomainLogic<Member>
    {
        public Member(User user, Community community)
        {
            this.Time = DateTime.UtcNow;
            this.UserId = user.Id;
            this.CommunityId = community.Id;
            this.Confirmed = community.Open;

            if (community.Open)
            {
                this.Access = AccessLevel.Member;
            }
            else
            {
                this.Access = AccessLevel.Other;
            }
        }

        public long UserId { get; private set; }
        public long CommunityId { get; private set; }
        public AccessLevel Access { get; set; }
        public bool Confirmed { get; set; }
        public DateTime Time { get; private set; }

        public User User()
        {
            return Repository<User>.FindFirstBy(x => x.Id == this.UserId);
        }

        public Community Community()
        {
            return Repository<Community>.FindFirstBy(x => x.Id == this.CommunityId);
        }
    }
}
