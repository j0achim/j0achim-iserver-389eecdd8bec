using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class RaidInfo : DomainLogic<RaidInfo>
    {
        public RaidInfo(Community community, string Name, string Description)
        {
            this.Name = Name;
            this.Description = Description;
            this.CommunityId = community.Id;
            this.Created = DateTime.UtcNow;
            this.AnnounceOnStart = true;
            this.AutoLock = false;
            this.LockAt = 0;
            this.Enabled = true;
        }

        public long CommunityId { get; set; }
        public bool AnnounceOnStart { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool AutoLock { get; set; }
        public bool Enabled { get; set; }
        public int LockAt { get; set; }
        public DateTime Created { get; set; }

        public IEnumerable<Raid> History()
        {
            return Repository<Raid>.FindBy(x => x.RaidInfoId == this.Id);
        }

        public Raid Start()
        {
            Raid raid = new Raid(this);
            raid.Save();

            return raid;
        }

        public void Enable()
        {
            if (Enabled)
                throw new Exception("Raid already enabled.");

            Enabled = true;
            Save();
        }

        public void Disable()
        {
            if (!Enabled)
                throw new Exception("Raid already disabled.");

            Enabled = false;
            Save();
        }
    }

    [LoadInfo]
    public class Raid : DomainLogic<Raid>
    {
        public Raid(RaidInfo raid)
        {
            this.Time = DateTime.UtcNow;
            this.RaidInfoId = raid.Id;
            this.Running = true;
            this.Locked = false;
        }

        public long RaidInfoId { get; set; }
        public bool Running { get; set; }
        public bool Locked { get; set; }
        public DateTime Time { get; set; }
        public DateTime EndTime { get; set; }

        public TimeSpan RunTime()
        {
            return (Running) ? (Time - DateTime.UtcNow) : (Time - EndTime);
        }

        public RaidInfo RaidInfo()
        {
            return Repository<RaidInfo>.FindSingleBy(x => x.Id == this.RaidInfoId);
        }

        public IEnumerable<RaidMember> Members()
        {
            return Repository<RaidMember>.FindBy(x => x.RaidId == this.Id);
        }

        public IEnumerable<RaidLeader> Leaders()
        {
            return Repository<RaidLeader>.FindBy(x => x.RaidId == this.Id);
        }

        public RaidMember GetMember(Character character)
        {
            return Members().Where(x => x.CharacterId == character.Id).FirstOrDefault();
        }

        public RaidMember UserJoin(Character character)
        {
            if (Locked)
                throw new Exception("Raid locked.");

            var m = GetMember(character);

            if(m!=null)
            {
                if(m.Kicked)
                {
                    throw new Exception("You cant join when you have been removed from raid.");
                }

                if(m.Left)
                {
                    m.Left = false;
                    m.Save();
                }
            }
            else
            {
                m = new RaidMember(this, character);
                m.Save();
            }

            AutoLock();

            return m;
        }

        public RaidMember UserLeave(Character character)
        {
            var m = GetMember(character);

            if(m==null)
            {
                throw new Exception(string.Format("You are not a member of this raid.", character.Nick));
            }
            else
            {
                if(m.Left)
                {
                    throw new Exception("You have already left this raid.");
                }
                else
                {
                    m.Left = true;
                    m.LeaveTime = DateTime.UtcNow;
                    m.Save();
                }
            }

            return m;
        }

        public RaidMember LeaderKickMember(Character character)
        {
            var m = GetMember(character);

            if (m == null)
            {
                throw new Exception(string.Format("{0} is not a member of this raid.", character.Nick));
            }
            else
            {
                m.Kicked = true;
                m.Save();
            }

            return m;
        }

        public RaidMember LeaderAddMember(Character character)
        {
            var m = GetMember(character);

            if (m == null)
            {
                m = new RaidMember(this, character);
                m.Save();
            }
            else
            {
                if(!m.Kicked && !m.Left)
                {
                    throw new Exception(string.Format("{0} is already a member of raid, and have not been kicked nor left raid.", character.Nick));
                }

                m.Left = false;
                m.Kicked = false;
                m.Save();
            }

            return m;
        }

        public RaidLeader GetLeader(Character character)
        {
            return Leaders().Where(x => x.CharacterId == character.Id).FirstOrDefault();
        }

        public RaidLeader LeaderAdd(Character character)
        {
            var leader = GetLeader(character);
            
            if(leader==null)
            {
                leader = new RaidLeader(this, character);
                leader.Save();
            }
            else
            {
                if(!leader.Left && !leader.Kicked)
                {
                    throw new Exception("Already a leader of this raid.");
                }

                leader.Left = false;
                leader.Kicked = false;
                leader.JoinTime = DateTime.UtcNow;
                leader.Save();
            }


            return leader;
        }

        public RaidLeader LeaderRemove(Character character)
        {
            var leader = GetLeader(character);

            if (leader == null)
            {
                throw new Exception("Is not a leader of this raid.");
            }
            else
            {
                if (leader.Left || leader.Kicked)
                {
                    throw new Exception("Already removed as leader of this raid.");
                }

                leader.Kicked = true;
                leader.LeaveTime = DateTime.UtcNow;
                leader.Save();
            }

            return leader;
        }

        public void AutoLock()
        {
            // No need to do any checking if raid is already locked.
            if (Locked)
                return;

            if(RaidInfo().AutoLock)
            {
                if(Members().Where(x=>!x.Left && !x.Kicked).Count() >= RaidInfo().LockAt)
                {
                    this.Locked = true;
                    Save();
                }
            }
        }

        public void Stop()
        {
            this.Running = false;
            this.EndTime = DateTime.UtcNow;

            Save();
        }
    }

    [LoadInfo]
    public class RaidMember : DomainLogic<RaidMember>
    {
        public RaidMember(Raid raid, Character character)
        {
            this.CharacterId = character.Id;
            this.JoinTime = DateTime.UtcNow;
            this.RaidId = raid.Id;
            this.Left = false;
            this.Kicked = false;
        }

        public long CharacterId { get; set; }
        public long RaidId { get; set; }
        public bool Left { get; set; }
        public bool Kicked { get; set; }
        public DateTime JoinTime { get; set; }
        public DateTime LeaveTime { get; set; }

        public Character Character()
        {
            return iRaidDatamodel.Entities.Character.FindFirst(x => x.CharacterId == CharacterId);
        }

        public Raid Raid()
        {
            return Repository<Raid>.FindSingleBy(x => x.Id == this.RaidId);
        }
    }

    [LoadInfo]
    public class RaidLeader : DomainLogic<RaidLeader>
    {
        public RaidLeader(Raid raid, Character character)
        {
            this.CharacterId = character.Id;
            this.JoinTime = DateTime.UtcNow;
            this.RaidId = raid.Id;
            this.Left = false;
            this.Kicked = false;
        }

        public long CharacterId { get; set; }
        public long RaidId { get; set; }
        public bool Left { get; set; }
        public bool Kicked { get; set; }
        public DateTime JoinTime { get; set; }
        public DateTime LeaveTime { get; set; }

        public Character Character()
        {
            return iRaidDatamodel.Entities.Character.FindFirst(x => x.CharacterId == CharacterId);
        }

        public Raid Raid()
        {
            return Repository<Raid>.FindSingleBy(x => x.Id == this.RaidId);
        }
    }
}
