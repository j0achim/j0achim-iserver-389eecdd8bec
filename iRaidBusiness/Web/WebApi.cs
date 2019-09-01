using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using iRaidDatamodel;
using iRaidDatamodel.Entities;
using iRaidApi;
using iRaidApi.Definitions;
using iRaidPoRK;
using iRaidTools;
using System.Reflection;

namespace iRaidBusiness
{
    public class WebApi
    {
        /// <summary>
        /// Connection / User object invoked method.
        /// </summary>
        private Connection sender;
        private WebContext context;
        private BusinessLogic d;
        private CoreApi Api;
        private User Annon = Repository<User>.FindFirstBy(x => x.UserName == "Anonymous");
        private User System = Repository<User>.FindFirstBy(x => x.UserName == "System");

        public WebApi(Connection connection, WebContext context)
        {
            this.context = context;
            this.sender = connection;
            this.Api = new CoreApi(this.sender);
            this.d = new BusinessLogic();
        }

        public string RunCommand(string json)
        {
            ReturnMessage r = new ReturnMessage();
            Stopwatch s = Stopwatch.StartNew();

            string msg;
            string action = "null";

            r.success = Api.Execute(json, typeof(WebApi), this, out msg, out action);
            r.action = action;
            r.reply = msg;

            s.Stop();
            r.executiontime = string.Format("Executed in {0} ms.", s.ElapsedMilliseconds);

            return JsonConvert.SerializeObject(r);
        }

        [Command(AuthRequired = false, Description = "Gets list of webpage menu.", Category = "WebSite")]
        public IEnumerable<WebMenu> GetMenu()
        {
            return WebMenu.All();
        }

        [Command(AuthRequired = true, Description = "Add or Saves menu item.", Category = "WebSite")]
        public WebMenu AddOrSaveMenu(string name, string icon, int pageid)
        {
            if (name.IsNullOrEmpty())
                throw new Exception("Name cannot be null or empty.");

            if (icon.IsNullOrEmpty())
                throw new Exception("Icon cannot be null or empty.");

            WebPage page = WebPage.FindFirst(x => x.Id == pageid);

            if (page == null)
                throw new Exception("Could not find webpage for that Id.");

            WebMenu menu = WebMenu.FindFirst(x => x.name == name);

            if (menu == null)
            {
                if (menu != null)
                    throw new Exception("Menu button with same name already exist.");

                menu = new WebMenu(pageid, name, icon);
            }
            else
            {
                menu.name = name;
                menu.icon = icon;
                menu.PageId = pageid;
            }

            menu.SaveMenu(sender.User);

            return menu;
        }

        [Command(AuthRequired = true, Description = "Adds or Saves webpage.", Category = "WebSite")]
        public WebPage AddOrSavePage(string title, string body, int pageid)
        {
            if (title.IsNullOrEmpty())
                throw new Exception("Name cannot be null or empty.");

            if (body.IsNullOrEmpty())
                throw new Exception("Conent cannot be null or empty.");

            WebPage page = WebPage.FindFirst(x => x.Id == pageid);

            if (page == null)
            {
                page = new WebPage(title, body);
            }
            else
            {
                page.Title = title;
                page.Body = body;
            }

            page.SavePage(sender.User);

            return page;
        }

        [Command(AuthRequired = false, Description = "Gets list of webpages.", Category = "WebSite")]
        public IEnumerable<WebPage> GetPages()
        {
            return WebPage.All();
        }

        [Command(AuthRequired = false, Description = "Authenticate with server.", Category="Account")]
        public int Auth(string username, string password)
        {
            if (sender.Auth)
            {
                throw new Exception("You are already logged in.");
            }

            if (username.IsNullOrEmpty() || password.IsNullOrEmpty())
            {
                throw new Exception("Name or Password cannot be null or empty.");
            }

            User u = null;
            if (User.TryGetUser(username, out u))
            {
                if (!u.Enabled)
                {
                    throw new Exception("Account is not Enabled.");
                }

                if (u.Login(password))
                {
                    sender.Auth = true;
                    sender.Name = u.UserName;
                    sender.User = u;

                    u.AddLog(u, LogAction.AuthSuccess, "Login From: " + sender.Access);
                }
                else
                {
                    u.AddLog(Annon, LogAction.AuthFailed, "Incorrect username or password: " + sender.Address);
                    throw new Exception("Incorrect UserName or Password.");
                }
            }
            else
            {
                throw new Exception("Incorrect UserName.");
            }

            return (int)sender.User.Access;
        }

        [Command(AuthRequired = false, Description = "Registers new account.", Category = "Account")]
        public string Register(string username, string password, string email, string name)
        {
            if (sender.Auth)
            {
                throw new Exception("You are already logged in.");
            }

            if (username.IsNullOrEmpty() || password.IsNullOrEmpty() || email.IsNullOrEmpty())
            {
                throw new Exception("Name, Password or Email cannot be null or empty.");
            }

            // Find user account.
            var u = User.FindFirst(x => x.UserName.ToLower() == username.ToLower());

            if (u == null)
            {
                u = new User(name, username, email, password);
                u.Save();

                sender.Auth = true;
                sender.Name = u.UserName;
                sender.User = u;
            }
            else
            {
                throw new Exception("UserName is already taken.");
            }

            return "Successfully registered, you are now logged in!";
        }

        [Command(AuthRequired = false, Description = "Reset password, sends user a password reset token to registered email.", Category = "Account")]
        public string ResetPassword(string email)
        {
            if (sender.Auth)
            {
                throw new Exception("You are already logged in.");
            }

            if (email.IsNullOrEmpty())
            {
                throw new Exception("Email cannot be null or empty.");
            }

            // Find user account.
            var u = User.FindFirst(x => x.Email.ToLower() == email.ToLower());

            if (u != null)
            {
                // Generate new reset password token.
            }
            else
            {
                throw new Exception("No account found with given Email address.");
            }

            return "Reset password token generated and sent, please check your email for password reset instructions.";
        }

        [Command(AuthRequired = true, Description = "Add new game.", Category = "Game")]
        public Game AddGame(string name)
        {
            return d.AddGame(name, sender.User);
        }

        [Command(AuthRequired = true, Description = "Rate a game by {id} with boolean {like}.", Category = "Game")]
        public void RateGame(int id, bool like)
        {
            d.RateGame(id, like, sender.User);
        }

        [Command(AuthRequired = false, Description = "Get list of games.", Category = "Game")]
        public IEnumerable<object> GetGames()
        {
            return d.GetGames();
        }

        [Command(AuthRequired = true, Description = "Add new community.", Category = "Community")]
        public Community AddCommunity(string name, string description, int gameid)
        {
            return d.AddCommunity(name, description, gameid, sender.User);
        }

        [Command(AuthRequired = true, Description = "Add new questioneer for {communityid} with {title} and {description}", Category = "Questioneer")]
        public Questioneer AddQuestioneer(int communityid, string title, string description)
        {
            return d.AddQuestioneer(communityid, title, description, sender.User);
        }

        [Command(AuthRequired = true, Description = "Gets list of questioneers for given {communityid}.", Category = "Questioneer")]
        public IEnumerable<Questioneer> GetQuestioneers(int communityid)
        {
            return d.GetQuestioneers(communityid);
        }

        [Command(AuthRequired = false, Description = "Returns an Exception.")]
        public object ExceptionTest()
        {
            return d.ExceptionTest();
        }

        [Command(AuthRequired = false, Description = "Returns list of all available Communities.", Category = "Community")]
        public List<Community> GetCommunities()
        {
            return d.GetCommunities();
        }

        [Command(AuthRequired = false, Description = "Get members of community {id}, {take} (1<>100) and number {skip} users from members database, use {skip} for pagination.", Category = "Community")]
        public IEnumerable<object> GetUsers(int id, int take, int skip)
        {
            return d.GetUsers(id, take, skip);
        }

        [Command(AuthRequired = true, Description = "Attempts to join a community.", Category = "Community")]
        public Community CommunityJoin(long CommunityId)
        {
            return null;
        }

        [Command(AuthRequired = true, Description = "Leaves a community, wallet will still be intact, so points are kept safe.", Category = "Community")]
        public Community CommunityLeave(long CommunityId)
        {
            return null;
        }

        [Command(AuthRequired = false, Description = "Returns list of active raids for community.", Category = "Community")]
        public object ActiveRaids(long CommunityId)
        {
            Community community;

            if (Community.FindCommunityById(CommunityId, out community))
            {
                return community.RaidRunning().Select(x => new { Raid = x, RaidInfo = x.RaidInfo() });
            }
            else
            {
                throw new Exception("Community not found.");
            }
        }

        [Command(AuthRequired = false, Description = "Get number {take} (1<>100) and number {skip} users who are currently online, use {skip} for pagination.")]
        public IEnumerable<object> Online(int take, int skip)
        {
            return d.Online(take, skip);
        }

        [Command(AuthRequired = false, Description = "Get number {take} (1<>100) and number {skip} item for game {gameid}, use {skip} for pagination.", Category = "Item")]
        public IEnumerable<object> GetItemsByGame(int gameid, int take, int skip)
        {
            return d.GetItemsByGame(gameid, take, skip);
        }

        [Command(AuthRequired = false, Description = "Returns information about item {id}.", Category = "Item")]
        public object GetItemById(int id)
        {
            return GetItemById(id);
        }

        [Command(AuthRequired = false, Description = "Search for item where name contains {name} finds the {num} (1<>100) number of results, optionally you can {skip} number of results for pagination.", Category = "Item")]
        public IEnumerable<object> FindItemByName(string name, int take = 25, int skip = 0)
        {
            return d.FindItemByName(name, take, skip);
        }

        [Command(AuthRequired = true, Description = "Sends a private message to another user.")]
        public string SendTo(string username, string message)
        {
            var u = CoreApi.ConnectionPool.Where(x => x.User.UserName.ToLower() == username.ToLower());

            if (u == null || u.Count() == 0)
            {
                throw new Exception("User not online.");
            }
            else
            {
                int s = 0;
                foreach (var user in u)
                {
                    if (user.ConnectionId == sender.ConnectionId)
                        continue;

                    s++;

                    context.SendTo(new PrivateMessage(sender.User.UserName, sender.ConnectionId, sender.UserId, message).Message(), user.ConnectionId);
                }

                if (s == 0)
                    throw new Exception("Message was not sent to anyone.");
            }

            return "Message sent!";
        }

        [Command(AuthRequired = false, Description = "Get wiki for given {communityid}.", Category = "Wiki")]
        public Wiki GetWiki(long CommunityId = 0)
        {
            return d.GetWiki(CommunityId);
        }

        [Command(AuthRequired = true, Description = "Adds a new Public Wiki page.", Category = "Wiki")]
        public Wiki AddWikiPage(string title, string body, long CommunityId = 0)
        {
            return d.AddWikiPage(title, body, sender.User, CommunityId);
        }

        [Command(AuthRequired = true, Description = "Saves a wiki page.", Category = "Wiki")]
        public Wiki SaveWikiPage(long Id, string Title, string Body)
        {
            Wiki wiki = Wiki.FindFirst(x => x.Id == Id);

            if (wiki != null)
            {
                wiki.Title = Title;
                wiki.Body = Body;
            }

            if(d.SaveWiki(wiki, sender.User) != null)
            {
                return wiki;
            }
            else
            {
                throw new Exception("Unable to save wiki page.");
            }
        }

        [Command(AuthRequired = false, Description = "List of public wiki Titles and Id's.", Category = "Wiki")]
        public IEnumerable<Wiki> GetWikiLinks(long CommunityId = 0)
        {
            return Wiki.Find(x => x.CommunityId == CommunityId);
        }

        [Command(AuthRequired = false, Description = "Gets specific Wiki page and it's content.", Category = "Wiki")]
        public Wiki GetWikiPageById(int id, long CommunityId = 0)
        {
            return d.GetWikiPageById(id, sender.Auth);
        }

        [Command(AuthRequired = false, Description = "Gets specific Wiki page and it's content, creates new page if none exist [User must be authenticated].", Category = "Wiki")]
        public Wiki GetWikiPageByTitle(string title, long CommunityId = 0)
        {
            return d.GetWikiPageByTitle(title, sender.Auth, sender.User, CommunityId);
        }

        [Command(AuthRequired = false, Description = "Display available methods through public API.", Category = "General")]
        public Help Help()
        {
            return Api.Help(typeof(WebApi));
        }

        [Command(AuthRequired = false, Description = "Returns List of schemas.", Category = "General")]
        public object GetSchema()
        {
            var generator = new JsonSchemaGenerator();

            dynamic schemalist = new List<dynamic>();

            foreach (var type in (from t in Assembly.GetAssembly(typeof(Repository<>)).GetTypes() where t.IsClass && t.Namespace == "iRaidDatamodel.Entities" select t).ToList())
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

                    schemalist.Add(generator.Generate(type));
                }
                catch
                {


                }
            }

            //var schema = generator.Generate(type);
            //schema.Title = type.Name;

            return null;
        }

        [Command(AuthRequired = false, Description = "Returns List of Characters (max 100) use take and page for pagination.", Category = "General")]
        public IEnumerable<Character> GetCharacters(int take, int page)
        {
            return d.GetCharacters(take, page);
        }

        [Command(AuthRequired = false, Description = "Trys to find Character returns list of found matches (max 100) use take and page for pagination.", Category = "General")]
        public IEnumerable<Character> FindCharacter(string name, int take, int page)
        {
            return d.FindCharacter(name, take, page);
        }

        [Command(AuthRequired = false, Description = "Returns character information about a single character.", Category = "General")]
        public Character FindSingleCharacter(string name)
        {
            var c = d.FindSingleCharacter(name);

            if (c == null)
                throw new Exception("Information is missing.");

            return c;
        }

        #region Templating

        [Command(AuthRequired = true, Description = "Adds new template.", Category = "Templating")]
        public Template TemplateAdd(long CommunityId, string Title, string Description, string Test, string Body, bool IsBotTemplate)
        {
            // Lets ensure runner has access to execute the command.
            d.AccessCheck(sender.User.Access, AccessLevel.SuperAdmin);

            if (Title.IsNullOrEmpty())
                throw new Exception("Title cannot be null or empty.");

            if (Body.IsNullOrEmpty())
                throw new Exception("Body cannot be null or empty.");

            if (CommunityId < 0)
                throw new Exception("Invalid CommunityId.");

            Template template = Template.FindFirst(x => x.CommunityId == CommunityId && x.Title.ToLower() == Title.ToLower());

            if (template != null)
                throw new Exception(string.Format("Template with same name already exist: {0}", Title));

            template = new Template(CommunityId, Body, Test, Title, Description, IsBotTemplate);
            template.SaveTemplate(sender.User);

            return template;
        }

        [Command(AuthRequired = true, Description = "Saves template.", Category = "Templating")]
        public Template TemplateSave(long Id, string Body, string Description, string Test, string Title)
        {
            // Lets ensure runner has access to execute the command.
            d.AccessCheck(sender.User.Access, AccessLevel.SuperAdmin);

            Template template = Template.FindFirst(x => x.Id == Id);

            if (template == null)
                throw new Exception("Template could not be found.");

            if(Title != template.Title)
            {
                // Lets make sure no other templates exist with same name before we edit it!
                if(Template.Find(x=>x.CommunityId == template.CommunityId && x.Id != template.Id && x.Title.ToLower() == Title.ToLower()).Any())
                {
                    throw new Exception("Cannot save template as one with same title already exist.");
                }
                else
                {
                    // We have ensured that no other template exist with same title.
                    template.Title = Title;
                }
            }

            template.Body = Body;
            template.Test = Test;
            template.Description = Description;

            template.SaveTemplate(sender.User);

            return template;
        }

        [Command(AuthRequired = true, Description = "Get List of available templates for community.", Category = "Templating")]
        public IEnumerable<Template> TemplateGet(long CommunityId)
        {
            // Lets ensure runner has access to execute the command.
            d.AccessCheck(sender.User.Access, AccessLevel.SuperAdmin);

            return Template.Find(x => x.CommunityId == CommunityId);
        }

        [Command(AuthRequired = true, Description = "Tries to find and run a template, with built in test.", Category = "Templating")]
        public string TemplateTest(long Id)
        {
            // Lets ensure runner has access to execute the command.
            d.AccessCheck(sender.User.Access, AccessLevel.SuperAdmin);

            Template template = Template.FindSingle(x => x.Id == Id);

            if (template == null)
                throw new Exception("Could not find template.");

            //var data = d.TemplateTest(template);

            var data = template.SelfTest();

            return data;
        }

        [Command(AuthRequired = true, Description = "Runs and returns result from template.", Category = "Templating")]
        public string TemplateRun(long Id, dynamic model)
        {
            Template template = Template.FindSingle(x => x.Id == Id);

            if (template == null)
                throw new Exception("Could not find template.");

            var data = d.TemplateRun(template, model);

            return data;
        }

        #endregion
    }
}
