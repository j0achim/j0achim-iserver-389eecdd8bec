using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class PrivateUserMessage : DomainLogic<PrivateUserMessage>
    {
        public PrivateUserMessage(User user, string title, string body)
        {
            this.Time = DateTime.UtcNow;
            this.UserId = user.Id;
            this.Read = false;
            this.Deleted = false;
            this.Title = title;
            this.Body = body;
        }

        public long UserId { get; private set; }
        public DateTime Time { get; set; }
        public bool Read { get; set; }
        public bool Deleted { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }

        /// <summary>
        /// User object.
        /// </summary>
        /// <returns></returns>
        public User User()
        {
            return Repository<User>.FindSingleBy(x => x.Id == this.UserId);
        }
    }
}
