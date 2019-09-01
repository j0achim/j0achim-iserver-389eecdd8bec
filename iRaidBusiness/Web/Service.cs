using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using iRaidApi;
using iRaidApi.Definitions;
using iRaidBusiness;

namespace iRaidBusiness
{
    public class WebContext : WebSocketBehavior
    {
        private WebApi Api;
        public Connection Connection;

        public WebContext()
        {
            //Console.WriteLine("New websocket thread initiated.");
        }


        public System.Net.IPAddress GetIP()
        {
            return this.Context.UserEndPoint.Address;
        }

        public IEnumerable<Connection> FindConnection(Func<Connection,bool> predicate)
        {
            return CoreApi.ConnectionPool.Where(predicate);
        }

        public void Braodcast(string message)
        {
            Sessions.Broadcast(message);
        }

        public void SendTo(string message, string id)
        {
            Sessions.SendTo(message, id);
        }

        public void SendMessage(string message)
        {
            Send(message);
        }

        protected override void OnOpen()
        {
            Connection = new Connection(this.ID, this, GetIP());
            Api = new WebApi(Connection, this);

            CoreApi.ConnectionPool.Add(Connection);
        }

        protected override void OnMessage (MessageEventArgs e)
        {
            Send(Api.RunCommand(e.Data.ToString()));
        }

        protected override void OnClose (CloseEventArgs e)
        {
            if (CoreApi.ConnectionPool.Contains(Connection))
            {
                CoreApi.ConnectionPool.Remove(Connection);
            }
        }
    }
}
