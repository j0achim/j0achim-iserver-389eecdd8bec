using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Collections.Concurrent;
using System.Diagnostics;
using iRaidDatamodel;
using iRaidDatamodel.Entities;
using iRaidWebsocket;
using iRaidApi;
using iRaidApi.Definitions;
using iRaidPoRK;
using System.Reflection;


namespace Test
{
    public class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            SocketServer server = new SocketServer();

            LoadCache("iRaidDatamodel.Entities");
            PreLoad();

            server.Start();

            stopwatch.Stop();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Server started in: {0}s.", Math.Round((double)stopwatch.ElapsedMilliseconds / 1000, 2));
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Gray;

            PoRK porkservice = new PoRK();
            porkservice.Start();

            PoRK.UserLookup("Dochere");
            PoRK.OrganizationLookup(500);

            bool run = true;

            while (run)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("Type help for available commands: ");

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                string cmd = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Gray;

                switch (cmd.ToLower())
                {
                    case "help":
                        Help();
                        break;
                    case "exit":
                        run = false;
                        break;
                    case "entities":
                        Cached("iRaidDatamodel.Entities");
                        break;
                    case "server stop":
                        server.Stop();
                        break;
                    case "server start":
                        server.Start();
                        break;
                    case "stats":
                        Stats(server);
                        break;
                    default:
                        break;
                }
            }
        }

        public static DateTime Time = DateTime.UtcNow;

        public static void PreLoad()
        {
            //Lets ensure we have a few generic system configurations set up!
            User Anonymous = Repository<User>.FindSingleBy(x => x.UserName == "Anonymous");
            if (Anonymous == null)
            {
                Anonymous = new User("Anonymous", "Anonymous", "Anonymous@iraid.net", "",false);
                Anonymous.Save();
            }

            User Guest = Repository<User>.FindSingleBy(x => x.UserName == "Guest");
            if (Guest == null)
            {
                Guest = new User("Guest", "Guest", "Guest@iraid.net", "", false);
                Guest.Save();
            }
            
            User System = Repository<User>.FindSingleBy(x => x.UserName == "System");
            if (System == null)
            {
                System = new User("System", "System", "System@iraid.net", "", false);
                System.Save();
            }

            User Admin = Repository<User>.FindSingleBy(x => x.UserName == "Admin");
            if (Admin == null)
            {
                Admin = new User("Admin", "Admin", "Admin@iraid.net", "", false);
                Admin.Save();
            }

            User SuperAdmin = Repository<User>.FindSingleBy(x => x.UserName == "SuperAdmin");
            if (SuperAdmin == null)
            {
                SuperAdmin = new User("SuperAdmin", "SuperAdmin", "SuperAdmin@iraid.net", "", false);
                SuperAdmin.Save();
            }

            User Administrator = Repository<User>.FindSingleBy(x => x.UserName == "Administrator");
            if (Administrator == null)
            {
                Administrator = new User("Administrator", "Administrator", "Administrator@iraid.net", "", false);
                Administrator.Save();
            }

            User iraid = Repository<User>.FindSingleBy(x => x.UserName == "iraid");
            if (iraid == null)
            {
                iraid = new User("iraid", "iraid", "iraid@iraid.net", "", false);
                iraid.Save();
            }

            User porkuser = Repository<User>.FindSingleBy(x => x.UserName == "PoRK");
            if (porkuser == null)
            {
                porkuser = new User("PoRK", "PoRK", "PoRK@iraid.net", "", false);
                porkuser.Save();
            }

            // Adding games.
            Game AnarchyOnline = Game.FindFirst(x => x.Name == "Anarchy Online");
            if (AnarchyOnline == null)
            {
                AnarchyOnline = new Game("Anarchy Online");
                AnarchyOnline.Save(System);
            }

            //// Adding communities.
            //Community iraidCommunity = Community.FindFirst(x => x.Name == "iraid");
            //if (iraidCommunity == null)
            //{
            //    iraidCommunity = new Community("iraid", "The Official iraid raiding community for Anarchy Online.", AnarchyOnline);
            //    iraidCommunity.Save(System);
            //}

            //// Adding communities.
            //Community iraidTestCommunity = Community.FindFirst(x => x.Name == "iraid ");
            //if (iraidTestCommunity == null)
            //{
            //    iraidTestCommunity = new Community("iraid", "The Official iraid Test raiding community for Anarchy Online.", AnarchyOnline);
            //    iraidTestCommunity.Save(System);
            //}

            //PoRK.OrganizationLookup(500);
            //PoRK.OrganizationLookup(4662);
            //PoRK.OrganizationLookup(4765);
            //PoRK.OrganizationLookup(9910);
            //PoRK.OrganizationLookup(10197);
            //PoRK.OrganizationLookup(4690);
            //PoRK.OrganizationLookup(9606);
            //PoRK.OrganizationLookup(9910);
            //PoRK.OrganizationLookup(389133);
            //PoRK.OrganizationLookup(9674);
            //PoRK.OrganizationLookup(4650);
            //PoRK.OrganizationLookup(4706);
            //PoRK.OrganizationLookup(4879);
            //PoRK.OrganizationLookup(4788);
            //PoRK.OrganizationLookup(4724);
            //PoRK.OrganizationLookup(376898);
            //porkservice.UserLookup("Arigold");
            //porkservice.UserLookup("Wisdomman");
            //porkservice.UserLookup("KItesenfo");
            //porkservice.UserLookup("Kitesnt");
            //porkservice.UserLookup("Cratalot");
        }

        public static void LoadCache(string NameSpace)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("");
            Console.WriteLine("Loading entities from Redis into Object-cache.");
            Console.WriteLine("");
            Console.WriteLine("Namespace: {0}", NameSpace);
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("----------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(" {0,-39} | {1,-7} | {2,6}", "Class Name", "Records", "Time");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("----------------------------------------------------------------");

            int loaded = 0;

            foreach (var type in (from t in Assembly.GetAssembly(typeof(Repository<>)).GetTypes() where t.IsClass && t.Namespace == NameSpace select t).ToList())
            {
                try
                {
                    //// Lets make sure Entity has attribute LoadInfo for default pre-caching.
                    LoadInfo info = type.GetCustomAttributes().OfType<LoadInfo>().Cast<LoadInfo>().FirstOrDefault();

                    if (info == null)
                    {
                        // Type has no LoadInfo skipping it.
                        continue;
                    }

                    var m = typeof(Repository<>).MakeGenericType(type).GetMethod("LoadIntoCache");

                    if (m != null)
                    {
                        m.Invoke(null, null);
                    }

                    loaded++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Exception when loading {0} into cache, exception: {1}\n{2}", type.Name, ex.Message, ex.InnerException));
                }
            }

            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("");
            Console.WriteLine("{0} Entities loaded into Object-cache.", loaded);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void Help()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("");
            Console.WriteLine(" Following commands are available.");
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(" ----------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  {0,-20} | {1,-30}", "Command", "Action");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(" ----------------------------------------------------------------");
            Console.WriteLine("  {0,-20} | {1,-30}", "Exit", "Closes the application.");
            Console.WriteLine("  {0,-20} | {1,-30}", "Entities", "Current number of cached object.");
            Console.WriteLine("  {0,-20} | {1,-30}", "Stats", "A few statistics for the server.");
            Console.WriteLine("  {0,-20} | {1,-30}", "Server Start", "Start WebSocket server.");
            Console.WriteLine("  {0,-20} | {1,-30}", "Server Stop", "Stop WebSocket server.");
            Console.WriteLine(" ----------------------------------------------------------------");
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void Stats(SocketServer server)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("");
            Console.WriteLine(" Statistics.");
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(" ----------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  {0,-20} | {1,-30}", "Stat", "Value");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(" ----------------------------------------------------------------");
            Console.WriteLine("  {0,-20} | {1,-30}", "Last Boot", Time.ToString());
            Console.WriteLine("  {0,-20} | {1,-30}", "Run Time", (DateTime.UtcNow-Time).ToString("c"));
            Console.WriteLine("  {0,-20} | {1,-30}", "Connections", CoreApi.ConnectionPool.Count);
            Console.WriteLine(" ----------------------------------------------------------------");
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void Cached(string NameSpace)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("");
            Console.WriteLine(" Objects currently cached in memory.");
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(" ----------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  {0,-39} | {1,-7}", "Object-type", "Records");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(" ----------------------------------------------------------------");

            foreach (var type in (from t in Assembly.GetAssembly(typeof(Repository<>)).GetTypes() where t.IsClass && t.Namespace == NameSpace select t).ToList())
            {
                try
                {
                    //// Lets make sure Entity has attribute LoadInfo for default pre-caching.
                    LoadInfo info = type.GetCustomAttributes().OfType<LoadInfo>().Cast<LoadInfo>().FirstOrDefault();

                    if (info == null)
                    {
                        // Type has no LoadInfo skipping it.
                        continue;
                    }

                    var m = typeof(Repository<>).MakeGenericType(type).GetMethod("Count");

                    if (m != null)
                    {
                        var r = m.Invoke(null, null);

                        if (r != null)
                        {
                            Console.WriteLine("  {0,-39} | {1,-7}", type.Name, r.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("  Exception when trying to count {0} from cache, exception: {1}", type.Name, ex.Message));
                }
            }

            Console.WriteLine(" ----------------------------------------------------------------");
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
