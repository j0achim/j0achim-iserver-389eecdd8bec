using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using iRaidApi;
using iRaidApi.Definitions;
using iRaidDatamodel;
using iRaidDatamodel.Entities;
using iRaidPoRK;
using Newtonsoft.Json;

namespace iRaidWebsocket.Services.BotApi
{
    class BotApi
    {
        /// <summary>
        /// Connection / User object invoked method.
        /// </summary>
        private Connection sender;
        private BotContext context;
        private CoreApi Api;

        public BotApi(Connection connection, BotContext context)
        {
            this.context = context;
            this.sender = connection;
            this.Api = new CoreApi(this.sender);
        }

        public string RunCommand(string json)
        {
            ReturnMessage r = new ReturnMessage();
            Stopwatch s = Stopwatch.StartNew();

            string msg;
            string action = "null";

            r.success = Api.Execute(json, typeof(BotApi), this, out msg, out action);
            r.action = action;
            r.reply = msg;

            s.Stop();
            r.executiontime = string.Format("Executed in {0} ms.", s.ElapsedMilliseconds);

            return JsonConvert.SerializeObject(r);
        }

        [Command(AuthRequired = false, Description = "Authenticate with server.", Category = "Account")]
        public void Auth(string BotName, string APIKey)
        {
            if (sender.Auth)
            {
                throw new Exception("You are already logged in.");
            }

            sender.Auth = true;
            sender.User = User.FindFirst(x => x.Name == "iraid");
        }

        [Command(AuthRequired = false, Description = "Friend event, user Logon / Logoff.", Category = "Friend")]
        public void FriendEvent(string Name, long Id, bool Online)
        {
            //Console.WriteLine("{0,-10}: {1,-15} ({2})", Id, Name, Online);
            PoRK.CharacterEvent(Name, Id, Online);
        }
    }
}
