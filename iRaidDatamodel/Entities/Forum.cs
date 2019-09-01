using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class Board : DomainLogic<Board>
    {
        public Board(Community community, string Name, string Description)
        {
            this.CommunoityId = community.Id;
            this.Title = Title;
            this.Description = Description;
            this.Created = DateTime.UtcNow;
            this.Deleted = false;
            this.Access = AccessLevel.Member;
        }

        public long CommunoityId { get; private set; }
        public DateTime Created { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }
        public AccessLevel Access { get; set; }

        public IEnumerable<Thread> Threads()
        {
            return Repository<Thread>.FindBy(x => x.BoardId == this.Id);
        }
    }

    [LoadInfo]
    public class Thread : DomainLogic<Thread>
    {
        public Thread(Board board, string Name, string Description)
        {
            this.BoardId = board.Id;
            this.Title = Title;
            this.Description = Description;
            this.Created = DateTime.UtcNow;
            this.Deleted = false;
        }

        public long BoardId { get; private set; }
        public DateTime Created { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }

        public IEnumerable<Post> Posts()
        {
            return Repository<Post>.FindBy(x => x.ThreadId == this.Id);
        }
    }

    [LoadInfo]
    public class Post : DomainLogic<Post>
    {
        public Post(Thread thread, string Name, string Description)
        {
            this.ThreadId = thread.Id;
            this.Title = Title;
            this.Description = Description;
            this.Created = DateTime.UtcNow;
            this.Deleted = false;
        }

        public long ThreadId { get; private set; }
        public DateTime Created { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }

        public Thread Thread()
        {
            return Repository<Thread>.FindSingleBy(x => x.Id == this.ThreadId);
        }
    }
}
