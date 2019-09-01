using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace iRaidApi.Definitions
{
    public class PrivateMessage
    {
        public PrivateMessage(string FromUser, string FromId, long FromUserId, string Message)
        {
            this.time = DateTime.UtcNow;
            this.fromid = FromId;
            this.fromuser = FromUser;
            this.fromuserid = FromUserId;
            this.message = Message;
        }

        public DateTime time { get; set; }
        public string fromuser { get; set; }
        public long fromuserid { get; set; }
        public string fromid { get; set; }
        public string message { get; set; }

        public string Message()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
