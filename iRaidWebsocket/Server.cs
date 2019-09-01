using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using WebSocketSharp.Net;
using iRaidApi;
using iRaidApi.Definitions;
using iRaidWebsocket.Services.BotApi;
using iRaidBusiness;

namespace iRaidWebsocket
{
    public class SocketServer
    {
        public SocketServer(int port = 8888)
        {
            server = new WebSocketServer(port);
        }

        public void Start()
        {
            if(!server.IsListening)
            {
                server = new WebSocketServer(8888);
                server.AddWebSocketService<WebContext>("/Web", () => new WebContext());
                server.AddWebSocketService<BotContext>("/Bot", () => new BotContext(this));

                // Flush Connections cache.
                CoreApi.ConnectionPool = new HashSet<Connection>();

                // Start server.
                server.Start();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("");
                Console.WriteLine("Websocket Server started!");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("----------------------------------------------------------------");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(" {0,-23} | {1,-4} | {2,-18}", "Address", "Port", "Path");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("----------------------------------------------------------------");

                foreach(var p in server.WebSocketServices.Paths)
                {
                    Console.WriteLine(" ws://{0,-18} | {1,-4} | {2,-18}", server.Address, server.Port, p);
                    //Console.WriteLine("- ws://{2}:{1}{0}", p, server.Port, server.Address);
                }

                Console.WriteLine("----------------------------------------------------------------");
                Console.WriteLine("");

                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("");
                Console.WriteLine("Websocket Server is already running!");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("----------------------------------------------------------------");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(" {0,-23} | {1,-4} | {2,-18}", "Address", "Port", "Path");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("----------------------------------------------------------------");

                foreach (var p in server.WebSocketServices.Paths)
                {
                    Console.WriteLine(" ws://{0,-18} | {1,-4} | {2,-18}", server.Address, server.Port, p);
                }

                Console.WriteLine("----------------------------------------------------------------");
                Console.WriteLine("");

                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void Stop()
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("");

            if(server.IsListening)
            {
                Console.WriteLine("Stopping Websocket Server...");

                // Stop server.
                server.Stop();

                if (!server.IsListening)
                {
                    Console.WriteLine("Server stopped!");
                }

                // Flush Connections cache.
                CoreApi.ConnectionPool = new HashSet<Connection>();
            }
            else
            {
                Console.WriteLine("Websocket Server not running.");
            }

            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public WebSocketServer server;
    }
}
