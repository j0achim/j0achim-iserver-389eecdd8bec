using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using iRaidDatamodel;
using iRaidDatamodel.Entities;
using iRaidTools;
using iRaidApi;
using iRaidApi.Definitions;
using iRaidTemplating;
using System.Dynamic;

namespace iRaidBusiness
{
    public class BusinessLogic
    {
        private User Annon = Repository<User>.FindFirstBy(x => x.UserName == "Anonymous");
        private User System = Repository<User>.FindFirstBy(x => x.UserName == "System");

        #region Uncategorized

        /// <summary>
        /// Gets list of currently authenticated users logged onto server.
        /// </summary>
        /// <param name="take"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public IEnumerable<object> Online(int take, int skip)
        {
            return CoreApi.ConnectionPool.Where(x => x.Auth).OrderBy(x => x.UserId).Skip(skip).Take(take.TakeDefault()).Select(x => new { UserName = x.User.UserName, UserId = x.UserId, ConnectionId = x.ConnectionId });
        }

        /// <summary>
        /// Returns an Exception.
        /// </summary>
        /// <returns></returns>
        public object ExceptionTest()
        {
            throw new Exception("This is a exception.");
        }

        /// <summary>
        /// Returns help for a given Type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Help Help(CoreApi api, Type type)
        {
            return api.Help(typeof(Type));
        }

        #endregion

        #region Anarchy Online

        public IEnumerable<Character> GetCharacters(int take, int page)
        {
            return Character.All().OrderBy(x => x.CharacterId).ThenBy(x => x.Nick).Skip(page.PageHelper(take)).Take(take.TakeDefault());
        }


        public IEnumerable<Character> FindCharacter(string name, int take, int page)
        {
            return Character.Find(x => CultureInfo.InvariantCulture.CompareInfo.IndexOf(x.Nick, name, CompareOptions.IgnoreCase) > -1).OrderBy(x => x.CharacterId).ThenBy(x => x.Nick).Skip(page.PageHelper(take)).Take(take.TakeDefault());
        }

        public Character FindSingleCharacter(string name)
        {
            var c = Character.FindFirst(x => x.Nick == name);

            if(c==null)
            {
                iRaidPoRK.PoRK.UserLookup(name);
            }

            return c;
        }

        public object CharacterStatistics(int orgid = 0)
        {
            var Characters = Character.All();

            if(orgid != 0)
            {
                Characters = Characters.Where(x => x.OrganizationId == orgid);
            }

            dynamic stats = new ExpandoObject();
            stats.Count = Characters.Count();
            stats.Roles = new ExpandoObject();
            stats.Roles.Unknown = Characters.Where(x => x.MainRole == "Unknown").Count();
            stats.Roles.MainTank = Characters.Where(x => x.MainRole == "Tank").Count();
            stats.Roles.MainDamage = Characters.Where(x => x.MainRole == "Damage").Count();
            stats.Roles.MainHealer = Characters.Where(x => x.MainRole == "Healer").Count();
            stats.Roles.SecondaryTank = Characters.Where(x => x.SecondaryRole == "Tank").Count();
            stats.Roles.SecondaryHealer = Characters.Where(x => x.SecondaryRole == "Healer").Count();
            stats.Roles.SecondaryDamage = Characters.Where(x => x.SecondaryRole == "Damage").Count();
            stats.Roles.SecondaryUtility = Characters.Where(x => x.SecondaryRole == "Utility").Count();

            return stats;
        }

        #endregion

        #region Games

        /// <summary>
        /// Checks if game exist and then tries to add new game.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public Game AddGame(string name, User sender)
        {
            if (sender == null)
            {
                throw new Exception("You have to login!");
            }

            Game game;

            if (Game.TryGetGame(name, out game))
            {
                throw new Exception("Game already exists.");
            }
            else
            {
                game = new Game(name);
                game.Save(sender);

                return game;
            }
        }

        /// <summary>
        /// Rates a game as either positive or negative.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="like"></param>
        /// <param name="User"></param>
        public void RateGame(int id, bool like, User User)
        {
            Game game;

            if (!Game.TryGetGame(id, out game))
            {
                throw new Exception(string.Format("Could not find game by id: {0}", id));
            }
            else
            {
                game.Rate(User, like);
            }
        }

        /// <summary>
        /// Returns a list of games and its communities.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> GetGames()
        {
            return Game.All().Select(x => new { Id = x.Id, Name = x.Name, Icon = x.Icon, Rating = x.Rating(), Communities = x.GetCommunities().ToList() });
        }

        #endregion

        #region Community

        public List<Community> GetCommunities()
        {
            return Community.All().ToList();
        }

        public Community AddCommunity(string name, string description, int gameid, User User)
        {
            Community community = null;
            Game game;
            if (!Game.TryGetGame(gameid, out game))
            {
                throw new Exception(string.Format("Could not find game by id: {0}", gameid));
            }
            else
            {
                if (Community.TryFindCommunity(name, game, out community))
                {
                    throw new Exception(string.Format("Community already exist with same name: {0}", name));
                }
                else
                {
                    community = new Community(name, description, game);
                    community.Save(User);
                    community.AddMember(User, System);
                }
            }

            return community;
        }

        public IEnumerable<object> GetUsers(int id, int take, int skip)
        {
            return User.All()
                   .Join(Member.Find(x => x.CommunityId == id).OrderBy(x => x.UserId).Skip(skip).Take(take.TakeDefault()),
                   post => post.Id,
                   meta => meta.UserId,
                   (post, meta) => new { UserId = post.Id, UserName = post.UserName }).OrderBy(x => x.UserId);

            //return (from u in User.All()
            //        join m in Member.Find(x => x.CommunityId == id).OrderBy(x => x.UserId).Skip(skip).Take(take.TakeDefault()).Select(x => x.UserId)
            //        on u.Id equals m
            //        select new { UserId = u.Id, UserName = u.UserName }).OrderBy(x => x.UserId);
        }

        #endregion

        #region Questioneer

        /// <summary>
        /// Adds a new questioneer for given {communityid} with {name} and {description}, added by {user}
        /// </summary>
        /// <param name="communityid"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="User"></param>
        /// <returns></returns>
        public Questioneer AddQuestioneer(int communityid, string title, string description, User User)
        {
            Questioneer questioneer;
            Community community;
            if (!Community.FindCommunityById(communityid, out community))
            {
                throw new Exception(string.Format("Community by id: {0} could not be found.", communityid));
            }
            else
            {
                questioneer = new Questioneer(community, title, description);
                questioneer.Save(User);
            }

            return questioneer;
        }

        /// <summary>
        /// Gets list of questioneers for given {communityid}
        /// </summary>
        /// <param name="communityid"></param>
        /// <returns></returns>
        public IEnumerable<Questioneer> GetQuestioneers(int communityid)
        {
            return Questioneer.Find(x => x.CommunityId == communityid && !x.Deleted);
        }

        #endregion

        #region Item

        public object GetItemById(int id)
        {
            var r = Item.Find(x => x.Id == id).Select(x => new { Name = x.Name, Properties = x.Properties() }).FirstOrDefault();

            if (r == null)
            {
                throw new Exception(string.Format("Could not find item by id: {0}", id));
            }
            else
            {
                return r;
            }
        }

        public IEnumerable<object> GetItemsByGame(int gameid, int take, int skip)
        {
            return Repository<Item>.FindBy(x => x.GameId == gameid).OrderBy(x => x.Id).Skip(skip).Take(take.TakeDefault());
        }

        public IEnumerable<object> FindItemByName(string name, int take = 25, int skip = 0)
        {
            var r = Item.Find(x => x.Name.ToLower().Contains(name.ToLower())).OrderBy(x => x.Id).Skip(skip).Take(take.TakeDefault()).Select(x => new { Name = x.Name, Properties = x.Properties() });

            return r;
        }

        #endregion

        #region  Wiki

        public Wiki AddWikiPage(string title, string body, User User, long CommunoityId = 0)
        {
            Wiki page = Wiki.FindFirst(x => x.Title.ToLower() == title.ToLower() && !x.Deleted);

            if (page != null)
            {
                throw new Exception("Wiki page with same name already exist.");
            }
            else
            {
                page = new Wiki(title, body);
                page.Save(User);
            }

            return page;
        }

        public IEnumerable<object> GetWikiLinks(long CommunityId = 0)
        {
            return Wiki.Find(x => x.CommunityId == 0 && !x.Deleted).OrderByDescending(x=>x.Index).ThenBy(x=>x.Title).Select(x => new { Id = x.Id, Name = x.Title, AuthRequired = x.AuthRequired, CommunityId = x.CommunityId, Index = x.Index });
        }

        public Wiki GetWikiPageById(int id, bool Auth, long CommunityId = 0)
        {
            Wiki page = Wiki.FindFirst(x => x.Id == id && !x.Deleted && x.CommunityId == CommunityId);

            if (page == null)
            {
                throw new Exception(string.Format("No Wiki page with id: {0} could be found.", id));
            }

            if (page.AuthRequired && Auth)
            {
                throw new Exception(string.Format("You need to login to view this page."));
            }

            return page;
        }

        public Wiki GetWikiPageByTitle(string title, bool Auth, User User, long CommunityId = 0)
        {
            Wiki page = Wiki.FindFirst(x => x.Title.ToLower() == title.ToLower() && !x.Deleted && x.CommunityId == CommunityId);

            if (page == null)
            {
                if (!Auth)
                {
                    throw new Exception(string.Format(string.Format("Page not found, login to automatically generate a page for: {0}", title)));
                }
                else
                {
                    page = new Wiki(title, "This is a empty page, start editing the page!", CommunityId);
                    page.Save(User);

                    return page;
                }
            }

            return page;
        }

        public Wiki SaveWiki(Wiki page, User User)
        {
            if (page == null)
                throw new Exception("page cannot be null");

            if (User == null)
                throw new Exception("User cannot be null");

            if (Wiki.FindFirst(x => x.Id == page.Id) == null)
                throw new Exception("Wiki Page does not exist.");

            page.Save(User);

            return page;
        }

        public Wiki GetWiki(long CommunityId)
        {
            Wiki page = Wiki.Find(x => x.CommunityId == CommunityId && !x.Deleted && !x.AuthRequired).OrderByDescending(x => x.Index).FirstOrDefault();

            return page;
        }

        /// <summary>
        /// Tries to run template with built in test.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public string TemplateTest(Template template)
        {
            // Lets get cache string.
            var cache = string.Format("{0}-{1}-{2}", template.Id, template.CommunityId, template.Version);

            // Lets get the dynamic object that we pass to template upon parsing.
            object model = JsonConvert.DeserializeObject<ExpandoObject>(template.Test);

            return template.Body.Compile((object)model, cache);
        }

        public string TemplateRun(Template template, dynamic model)
        {
            // Lets get cache string.
            var cache = string.Format("{0}-{1}-{2}", template.Id, template.CommunityId, template.Version);

            // Lets get the dynamic object that we pass to template upon parsing.
            object Model = JsonConvert.DeserializeObject<ExpandoObject>(model);

            return template.Body.Compile((object)Model, cache);
        }

        public Template TemplateSelfTest(Template template)
        {
            template.SelfTest();
            return template;
        }

        public bool AccessCheck(AccessLevel user, AccessLevel required)
        {
            if (user >= required)
                return true;
            else
                throw new Exception(string.Format("Insufficient access, your access level {0} required access level {1}", user.ToString(), required.ToString()));
        }

        #endregion
    }
}
