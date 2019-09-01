using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using iRaidApi.Definitions;
using iRaidWebsocket.Services.BotApi;

namespace iRaidWebsocket.Services.BotApi
{
    public class BotContext : WebSocketBehavior
    {
        public SocketServer server;
        private BotApi Api;
        public Connection Connection;

        private object SyncLock = new object();

        public BotContext(SocketServer server)
        {
            Console.WriteLine("New websocket thread initiated.");
            this.server = server;
        }

        protected override void OnOpen()
        {
            Api = new BotApi(Connection, this);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            lock (SyncLock)
            {
                try
                {
                    //Console.WriteLine(e.Data.ToString());

                    Api.RunCommand(e.Data.ToString());

                    //Send(Api.RunCommand(e.Data.ToString()));
                }
                catch
                {

                }
            }
            //Sessions.Broadcast(e.Data);
        }
    }
}
