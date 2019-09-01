using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using iRaidDatamodel;
using iRaidDatamodel.Entities;
using iRaidWebsocket;
using iRaidBusiness;
using iRaidTools;

namespace UnitTesting
{
    [TestClass]
    public class iServerUnitTest
    {
        private SocketServer server = null;
        object SyncLock = new object();

        private void Prepping()
        {
            lock (SyncLock)
            {
                if (server == null)
                {
                    server = new SocketServer(9999);
                    server.Start();

                    LoadCache("iRaidDatamodel.Entities");

                    // Test Account

                    User TestAccount = User.FindSingle(x => x.UserName == "UnitTest");

                    if(TestAccount == null)
                    {
                        TestAccount = new User("Unit Test User", "UnitTest", "UnitTester@iraid.io", "UnitTest", false);
                        TestAccount.Save();
                    }
                    else
                    {
                        TestAccount.Tokens = 0;
                        TestAccount.Enabled = false;
                        TestAccount.Save();

                        // Lets remove any old log records.

                        var logs = Log.Find(x => x.UserId == TestAccount.Id).ToList();

                        foreach(var l in logs)
                        {
                            Repository<Log>.Remove(l);
                        }
                    }
                }
            }
        }

        private void LoadCache(string NameSpace)
        {
            foreach (var type in (from t in Assembly.GetAssembly(typeof(Repository<>)).GetTypes() where t.IsClass && t.Namespace == NameSpace select t).ToList())
            {
                // Lets make sure Entity has attribute LoadInfo for default pre-caching.
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
            }
        }

        [TestMethod]
        public void Warmup()
        {
            // Make sure system are go.
            Prepping();
        }

        [TestMethod]
        public void ToolsToUnixTimeStamp()
        {
            DateTime now = DateTime.UtcNow;
            long time = now.ToUnixTimestamp();
        }

        [TestMethod]
        public void ToolsSecondsToDateTime()
        {
            long time = 0;
            DateTime date = time.SecondsToDateTime();
        }

        [TestMethod]
        public void UserTryGetUserByString()
        {
            User user;

            if(!User.TryGetUser("UnitTest", out user))
            {
                throw new Exception("Not warmed up yet?");
            }
            else
            {
                Console.WriteLine("Found user: {0} ({1})", user.Id, user.UserName);
            }
        }

        [TestMethod]
        public void UserTryGetUserById()
        {
            User user;

            if (!User.TryGetUser(1, out user))
            {
                throw new Exception("Not warmed up yet?");
            }
            else
            {
                Console.WriteLine("Found user: {0} ({1})", user.Id, user.UserName);
            }
        }

        [TestMethod]
        public void UserLogin()
        {
            User user;

            if(User.TryGetUser("UnitTest", out user))
            {
                if(!user.Login("UnitTest"))
                {
                    throw new Exception("Unable to login incorrect password for user UnitTest password UnitTest?");
                }
            }
            else
            {
                throw new Exception("User not found wamred up yet?");
            }
        }

        [TestMethod]
        public void UserChangePassword()
        {
            User user;

            if (User.TryGetUser("UnitTest", out user))
            {
                if (!user.ChangePassword("UnitTest","UnitTest"))
                {
                    throw new Exception("Unable to change password, incorrect password for user UnitTest password UnitTest?");
                }
            }
            else
            {
                throw new Exception("User not found wamred up yet?");
            }
        }

        [TestMethod]
        public void UserTokenAdd()
        {
            User user;

            if (User.TryGetUser("UnitTest", out user))
            {
                user.AddTokens(10, user);

                Assert.AreEqual(10, user.Tokens);
            }
            else
            {
                throw new Exception("User not found wamred up yet?");
            }
        }

        [TestMethod]
        public void UserTokenRemove()
        {
            User user;

            if (User.TryGetUser("UnitTest", out user))
            {
                user.RemoveTokens(10, user);

                Assert.AreEqual(0, user.Tokens);
            }
            else
            {
                throw new Exception("User not found wamred up yet?");
            }
        }

        [TestMethod]
        public void GetCharacters()
        {
            BusinessLogic b = new BusinessLogic();

            var c = b.GetCharacters(1003, 0);

            Assert.AreEqual(100, c.Count());
        }

        [TestMethod]
        public void FindCharacter()
        {
            BusinessLogic b = new BusinessLogic();

            var Characters = b.FindCharacter("Kites",100, 0);

            foreach (var c in Characters)
            {
                Console.WriteLine("{0} ({1})", c.Nick, c.CharacterId);
            }
        }

        [TestMethod]
        public void CharacterStatistics()
        {
            BusinessLogic b = new BusinessLogic();

            var stats = b.CharacterStatistics();
        }

        [TestMethod]
        public void CommunityNew()
        {
            Game game;

            if(Game.TryGetGame("Anarchy Online", out game))
            {
                Community community;

                if(Community.TryFindCommunity("Iraid Test", game, out community))
                {
                    // Community found!
                }
                else
                {
                    // Add new Community.
                    community = new Community("Iraid Test", "Iraid Test Environment.", game);
                    community.Save();
                }
            }
            else
            {
                throw new Exception("Game not found not warmed up yet?");
            }
        }

        [TestMethod]
        public void RaidAdd()
        {
            Game game;

            if (Game.TryGetGame("Anarchy Online", out game))
            {
                Community community;

                if (Community.TryFindCommunity("Iraid Test", game, out community))
                {
                    RaidInfo raid;

                    if(!community.TryFindRaid("Test Raid 1", out raid))
                    {
                        raid = community.RaidAdd("Test Raid 1", "This is a Test Raid");

                        Console.WriteLine("Raid successfully added.");
                    }
                    else
                    {
                        Console.WriteLine("Raid already exist.");
                    }
                }
            }
            else
            {
                throw new Exception("Game not found not warmed up yet?");
            }
        }

        [TestMethod]
        public void RaidTest()
        {
            Game game;

            if (Game.TryGetGame("Anarchy Online", out game))
            {
                Community community;

                if (Community.TryFindCommunity("Iraid Test", game, out community))
                {
                    // Stopping any previously running raids.
                    IEnumerable<Raid> Raids = community.RaidRunning();

                    foreach(var r in Raids.ToList())
                    {
                        r.Stop();
                    }

                    RaidInfo raid;                    
                    
                    if(community.TryFindRaid("Test Raid 1", out raid))
                    {
                        Raid r = raid.Start();

                        r.UserJoin(Character.FindFirst(x => x.Nick == "Kitessolja"));
                        r.UserLeave(Character.FindFirst(x => x.Nick == "Kitessolja"));
                        r.LeaderAddMember(Character.FindFirst(x => x.Nick == "Kitessolja"));
                        r.LeaderKickMember(Character.FindFirst(x => x.Nick == "Kitessolja"));

                        Console.WriteLine("Raid Successfully started.");

                        //r.Stop();
                    }
                    else
                    {
                        throw new Exception("RaidInfo not found, not warmed up yet?");
                    }
                }
                else
                {
                    throw new Exception("Community not found, not warmed up yet?");
                }
            }
            else
            {
                throw new Exception("Game not found, not warmed up yet?");
            }
        }
    }
}
