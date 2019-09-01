//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Diagnostics;
//using iRaidApi;
//using iRaidApi.Definitions;
//using Datamodel;
//using Datamodel.Entities;
//using Newtonsoft.Json;
//using WebSocketSharp;

//namespace iRaidWebsocket.Services.WebApis
//{
//    //public class WebApi
//    //{
//    //    /// <summary>
//    //    /// Connection / User object invoked method.
//    //    /// </summary>
//    //    private Connection sender;
//    //    private WebContext context;
//    //    private CoreApi Api;
//    //    private User Annon = Repository<User>.FindFirstBy(x => x.UserName == "Anonymous");
//    //    private User System = Repository<User>.FindFirstBy(x => x.UserName == "System");

//    //    public WebApi(Connection connection, WebContext context)
//    //    {
//    //        this.context = context;
//    //        this.sender = connection;
//    //        this.Api = new CoreApi(this.sender);
//    //    }

//    //    public string RunCommand(string json)
//    //    {
//    //        ReturnMessage r = new ReturnMessage();
//    //        Stopwatch s = Stopwatch.StartNew();

//    //        string msg;
//    //        string action = "null";

//    //        r.success = Api.Execute(json, typeof(WebApi), this, out msg, out action);
//    //        r.action = action;
//    //        r.reply = msg;

//    //        s.Stop();
//    //        r.executiontime = string.Format("Executed in {0} ms.", s.ElapsedMilliseconds);

//    //        return JsonConvert.SerializeObject(r);
//    //    }

//    //    [Command(AuthRequired = false, Description = "Authenticate with server.")]
//    //    public string Auth(string username, string password)
//    //    {
//    //        if (sender.Auth)
//    //        {
//    //            throw new Exception("You are already logged in.");
//    //        }

//    //        if (username.IsNullOrEmpty() || password.IsNullOrEmpty())
//    //        {
//    //            throw new Exception("Name or Password cannot be null or empty.");
//    //        }

//    //        User u = null;
//    //        if (User.TryGetUser(username, out u))
//    //        {
//    //            if(!u.Enabled)
//    //            {
//    //                throw new Exception("Account is not Enabled.");
//    //            }

//    //            if (u.Login(password))
//    //            {
//    //                sender.Auth = true;
//    //                sender.Name = u.UserName;
//    //                sender.User = u;

//    //                u.AddLog(u, LogAction.AuthSuccess, "Login From: " + context.Context.UserEndPoint.Address);
//    //            }
//    //            else
//    //            {
//    //                u.AddLog(Annon, LogAction.AuthFailed, "Incorrect username or password: " + context.Context.UserEndPoint.Address);
//    //                throw new Exception("Incorrect UserName or Password.");
//    //            }
//    //        }
//    //        else
//    //        {
//    //            throw new Exception("Incorrect UserName.");
//    //        }

//    //        return "Successfully logged in!";
//    //    }

//    //    [Command(AuthRequired = false, Description = "Registers new account.")]
//    //    public string Register(string username, string password, string email, string name)
//    //    {
//    //        if (sender.Auth)
//    //        {
//    //            throw new Exception("You are already logged in.");
//    //        }

//    //        if (username.IsNullOrEmpty() || password.IsNullOrEmpty() || email.IsNullOrEmpty())
//    //        {
//    //            throw new Exception("Name, Password or Email cannot be null or empty.");
//    //        }

//    //        // Find user account.
//    //        var u = User.FindFirst(x => x.UserName.ToLower() == username.ToLower());

//    //        if (u == null)
//    //        {
//    //            u = new User(name, username, email, password);
//    //            u.Save();

//    //            sender.Auth = true;
//    //            sender.Name = u.UserName;
//    //            sender.User = u;
//    //        }
//    //        else
//    //        {
//    //            throw new Exception("UserName is already taken.");
//    //        }

//    //        return "Successfully registered, you are now logged in!";
//    //    }

//    //    [Command(AuthRequired = true, Description = "Add new game.")]
//    //    public Game AddGame(string name)
//    //    {
//    //        Game game;

//    //        if (Game.TryGetGame(name, out game))
//    //        {
//    //            throw new Exception("Game already exists.");
//    //        }
//    //        else
//    //        {
//    //            game = new Game(name);
//    //            game.Save(sender.User);

//    //            return game;
//    //        }
//    //    }

//    //    [Command(AuthRequired = true, Description = "Rate a game by {id} with boolean {like}.")]
//    //    public void RateGame(int id, bool like)
//    //    {
//    //        Game game;

//    //        if (!Game.TryGetGame(id, out game))
//    //        {
//    //            throw new Exception(string.Format("Could not find game by id: {0}", id));
//    //        }
//    //        else
//    //        {
//    //            game.Rate(sender.User, like);
//    //        }
//    //    }

//    //    [Command(AuthRequired = false, Description = "Get list of games.")]
//    //    public IEnumerable<object> GetGames()
//    //    {
//    //        return Game.All().Select(x => new { Id = x.Id, Name = x.Name, Icon = x.Icon, Rating = x.Rating(), Communities = x.GetCommunities().ToList() });
//    //    }

//    //    [Command(AuthRequired = true, Description = "Add new community.")]
//    //    public Community AddCommunity(string name, string description, int gameid)
//    //    {
//    //        Community community = null;
//    //        Game game;
//    //        if (!Game.TryGetGame(gameid, out game))
//    //        {
//    //            throw new Exception(string.Format("Could not find game by id: {0}", gameid));
//    //        }
//    //        else
//    //        {
//    //            if(Community.TryFindCommunity(name, game, out community))
//    //            {
//    //                throw new Exception(string.Format("Community already exist with same name: {0}", name));
//    //            }
//    //            else
//    //            {
//    //                community = new Community(name, description, game);
//    //                community.Save(sender.User);
//    //                community.AddMember(sender.User, System);
//    //            }
//    //        }

//    //        return community;
//    //    }

//    //    [Command(AuthRequired = true, Description = "Add new questioneer for {communityid} with {title} and {description}")]
//    //    public Questioneer AddQuestioneer(int communityid, string title, string description)
//    //    {
//    //        Questioneer questioneer;
//    //        Community community;
//    //        if(!Community.FindCommunityById(communityid, out community))
//    //        {
//    //            throw new Exception(string.Format("Community by id: {0} could not be found.", communityid));
//    //        }
//    //        else
//    //        {
//    //            questioneer = new Questioneer(community, title, description);
//    //            questioneer.Save(sender.User);
//    //        }

//    //        return questioneer;
//    //    }

//    //    [Command(AuthRequired = false, Description = "Returns an Exception.")]
//    //    public object ExceptionTest()
//    //    {
//    //        throw new Exception("This is a exception.");

//    //        //return new { Message = "Hello world!", Time = DateTime.UtcNow };
//    //    }

//    //    [Command(AuthRequired = false, Description = "Returns list of all available Communities.")]
//    //    public List<Community> GetCommunities()
//    //    {
//    //        return Community.All().ToList();
//    //    }

//    //    [Command(AuthRequired = false, Description = "Get numbers of community {id}, {take} (1<>100) and number {skip} users from members database, use {skip} for pagination.")]
//    //    public IEnumerable<object> GetUsers(int id ,int take, int skip)
//    //    {
//    //        if (take > 100)
//    //        {
//    //            take = 100;
//    //        }

//    //        if (take <= 0)
//    //        {
//    //            take = 1;
//    //        }

//    //        return (from u in User.All()
//    //                join m in Member.Find(x => x.CommunityId == id).OrderBy(x => x.UserId).Skip(skip).Take(take).Select(x => x.UserId)
//    //                on u.Id equals m
//    //                select new { UserId = u.Id, UserName = u.UserName }).OrderBy(x => x.UserId);
//    //    }

//    //    [Command(AuthRequired = false, Description = "Get number {take} (1<>100) and number {skip} users who are currently online, use {skip} for pagination.")]
//    //    public IEnumerable<object> Online(int take, int skip)
//    //    {
//    //        if (take > 100)
//    //        {
//    //            take = 100;
//    //        }

//    //        if (take <= 0)
//    //        {
//    //            take = 1;
//    //        }

//    //        return CoreApi.ConnectionPool.OrderBy(x => x.UserId).Skip(skip).Take(take).Select(x => new { UserName = x.User.UserName, UserId = x.UserId, ConnectionId = x.ConnectionId });
//    //    }

//    //    [Command(AuthRequired = false, Description = "Get number {take} (1<>100) and number {skip} item for game {gameid}, use {skip} for pagination.")]
//    //    public IEnumerable<object> GetItemsByGame(int gameid, int take, int skip)
//    //    {
//    //        if (take > 100)
//    //        {
//    //            take = 100;
//    //        }

//    //        if (take <= 0)
//    //        {
//    //            take = 1;
//    //        }

//    //        return Repository<Item>.FindBy(x => x.GameId == gameid).OrderBy(x=>x.Id).Skip(skip).Take(take);
//    //    }

//    //    [Command(AuthRequired = false, Description = "Returns information about item {id}.")]
//    //    public object GetItemById(int id)
//    //    {
//    //        var r = Item.Find(x => x.Id == id).Select(x => new { Name = x.Name, Properties = x.Properties() }).FirstOrDefault();

//    //        if (r == null)
//    //        {
//    //            throw new Exception(string.Format("Could not find item by id: {0}", id));
//    //        }
//    //        else
//    //        {
//    //            return r;
//    //        }
//    //    }

//    //    [Command(AuthRequired = false, Description = "Search for item where name contains {name} finds the {num} (1<>100) number of results, optionally you can {skip} number of results for pagination.")]
//    //    public IEnumerable<object> FindItemByName(string name, int take = 25, int skip = 0)
//    //    {
//    //        if (take > 100)
//    //        {
//    //            take = 100;
//    //        }

//    //        if (take <= 0)
//    //        {
//    //            take = 1;
//    //        }

//    //        var r = Item.Find(x => x.Name.ToLower().Contains(name.ToLower())).OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => new { Name = x.Name, Properties = x.Properties() });

//    //        return r;
//    //    }

//    //    [Command(AuthRequired = true, Description = "Sends a private message to another user.")]
//    //    public string SendTo(string username, string message)
//    //    {
//    //        var u = CoreApi.ConnectionPool.Where(x => x.User.UserName.ToLower() == username.ToLower());

//    //        if (u == null || u.Count() == 0)
//    //        {
//    //            throw new Exception("User not online.");
//    //        }
//    //        else
//    //        {
//    //            int s = 0;
//    //            foreach (var user in u)
//    //            {
//    //                if (user.ConnectionId == sender.ConnectionId)
//    //                    continue;

//    //                s++;

//    //                context.SendTo(new PrivateMessage(sender.User.UserName, sender.ConnectionId, sender.UserId, message).Message(), user.ConnectionId);
//    //            }

//    //            if (s == 0)
//    //                throw new Exception("Message was not sent to anyone.");
//    //        }

//    //        return "Message sent!";
//    //    }

//    //    [Command(AuthRequired = true, Description = "Adds a new Public Wiki page.")]
//    //    public Wiki AddWikiPage(string title, string body)
//    //    {
//    //        Wiki page = Wiki.FindFirst(x => x.Title.ToLower() == title.ToLower() && !x.Deleted);

//    //        if(page != null)
//    //        {
//    //            throw new Exception("Wiki page with same name already exist.");
//    //        }
//    //        else
//    //        {
//    //            page = new Wiki(title, body);
//    //            page.Save(sender.User);
//    //        }

//    //        return page;
//    //    }

//    //    [Command(AuthRequired = false, Description = "List of public wiki Titles and Id's.")]
//    //    public IEnumerable<object> GetWikiLinks()
//    //    {
//    //        return Wiki.Find(x => x.CommunityId == 0 && !x.Deleted).Select(x => new { Id = x.Id, Name = x.Title, AuthRequired = x.AuthRequired });
//    //    }

//    //    [Command(AuthRequired = false, Description = "Gets specific Wiki page and it's content.")]
//    //    public object GetWikiPageById(int id)
//    //    {
//    //        Wiki page = Wiki.FindFirst(x => x.Id == id && !x.Deleted);

//    //        if(page == null)
//    //        {
//    //            throw new Exception(string.Format("No Wiki page with id: {0} could be found.", id));
//    //        }

//    //        if(page.AuthRequired && sender.Auth)
//    //        {
//    //            throw new Exception(string.Format("You need to login to view this page."));
//    //        }

//    //        return new { Id = page.Id, Title = page.Title, Body = page.Body, Log = page.GetLog() };
//    //    }

//    //    [Command(AuthRequired = false, Description = "Gets specific Wiki page and it's content, creates new page if none exist [User must be authenticated].")]
//    //    public object GetWikiPageByTitle(string title)
//    //    {
//    //        Wiki page = Wiki.FindFirst(x => x.Title.ToLower() == title.ToLower() && !x.Deleted);

//    //        if (page != null)
//    //        {
//    //            return new { Id = page.Id, Title = page.Title, Body = page.Body, Log = page.GetLog() };
//    //        }
//    //        else
//    //        {
//    //            if(!sender.Auth)
//    //            {
//    //                throw new Exception(string.Format(string.Format("Page not found, login to automatically generate a page for: {0}", title)));
//    //            }
//    //            else
//    //            {
//    //                page = new Wiki(title, "This is a empty page, start editing the page!");
//    //                page.Save(sender.User);

//    //                return new { Id = page.Id, Title = page.Title, Body = page.Body, Log = page.GetLog() };
//    //            }
//    //        }
//    //    }

//    //    [Command(AuthRequired = false, Description = "Display available methods through public API.")]
//    //    public Help Help()
//    //    {
//    //        return Api.Help(typeof(WebApi));
//    //    }
//    //}
//}
