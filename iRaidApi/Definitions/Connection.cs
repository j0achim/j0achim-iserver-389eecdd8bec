using System;
using iRaidDatamodel;
using iRaidDatamodel.Entities;
using System.Net;

namespace iRaidApi.Definitions
{
    public class Connection
    {
        public Connection(string ConnectionId, object Context, IPAddress address)
        {
            this.Context = Context;
            this.ConnectionId = ConnectionId;
            this.Name = "Not Authenticated";
            this.Auth = false;
            this.Time = DateTime.UtcNow;
            this.Access = AccessLevel.Member;
            this.Address = address;
        }

        private object Context;

        public T GetContext<T>() where T : class
        {
            return Context as T;
        }

        public Type Type()
        {
            return Context.GetType();
        }

        public long UserId { get; set; }

        public User User
        {
            get
            {
                return Repository<User>.FindFirstBy(x => x.Id == this.UserId);
            }
            set
            {
                this.UserId = value.Id;
            }
        }
        public AccessLevel Access { get; set; }
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public bool Auth { get; set; }
        public DateTime Time { get; set; }
        public IPAddress Address { get; set; }
    }
}
