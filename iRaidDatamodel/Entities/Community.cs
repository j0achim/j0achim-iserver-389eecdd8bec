using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class Community : DomainLogic<Community>
    {
        public Community(string Name, string Description, Game game)
        {
            this.Open = true;
            this.Guild = false;
            this.GameId = game.Id;
            this.Name = Name;
            this.Description = Description;
        }

        public long GameId { get; private set; }
        public bool Guild { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Open { get; set; }

        public IEnumerable<User> Members()
        {
            return from user in User.All()
                   join member in Member.Find(x => x.CommunityId == this.Id && x.Confirmed) 
                   on user.Id equals member.UserId
                   select user;
        }

        public IEnumerable<User> UnconfirmedMembers()
        {
            return from user in User.All()
                   join member in Member.Find(x => x.CommunityId == this.Id && !x.Confirmed)
                   on user.Id equals member.UserId
                   select user;
        }

        public bool AddMember(User user, User addedby)
        { 
            // Make sure we dont get double entries.
            if (!Repository<Member>.Contains(new Member(user, this)))
            {
                Member newMember = new Member(user, this);
                newMember.Save(addedby);
            }
            else
            {
                return false;
            }

            return true;
        }

        public static bool FindCommunityById(long id, out Community community)
        {
            community = Repository<Community>.FindFirstBy(x => x.Id == id);

            if(community == null)
            {
                return false;
            }

            return true;
        }

        public static bool TryFindCommunity(string name, Game game, out Community community)
        {
            community = null;
            if (game == null)
            {
                return false;
            }

            community = Repository<Community>.FindSingleBy(x => x.Name.ToLower() == name.ToLower() && x.GameId == game.Id);

            if(community == null)
            {
                return false;
            }

            return true;
        }

        public bool TryFindRaid(string name, out RaidInfo raid)
        {
            raid = RaidInfo.FindFirst(x => x.CommunityId == this.Id && x.Name.ToLower() == name.ToLower());

            if (raid == null)
                return false;

            return true;
        }

        public RaidInfo RaidAdd(string name, string description)
        {
            RaidInfo raid;

            if(!TryFindRaid(name, out raid))
            {
                raid = new RaidInfo(this, name, description);
                raid.Save();

                return raid;
            }
            else
            {
                throw new Exception("Raid already exist with same name.");
            }
        }

        public IEnumerable<Raid> RaidRunning()
        {
            return Raid.Find(x => x.Running && x.RaidInfo().CommunityId == this.Id);
        }
    }

    [LoadInfo]
    public class CommunityAccess : DomainLogic<CommunityAccess>
    {
        public CommunityAccess(User user, AccessLevel access)
        {
            UserId = user.Id;
            Access = access;
        }

        public long UserId { get; set; }
        public AccessLevel Access {get;set;}
    }
}
