using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iRaidDatamodel;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class Game : DomainLogic<Game>
    {
        public Game(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }

        public Rating Rating()
        {
            return Like.GetRating(typeof(Game), this.Id);
        }

        public IEnumerable<Community> GetCommunities()
        {
            return Repository<Community>.FindBy(x => x.GameId == this.Id);
        } 

        public void Rate(User user, bool liked)
        {
            Like like = null;

            if (Like.GetByUserId(typeof(Game), this.Id, user, out like))
            {
                if (like.Liked != liked)
                {
                    like.Rate(liked);
                    like.Save(user);
                }
            }
            else
            {
                like = new Like(typeof(Game), this.Id, user, liked);
                like.Save(user);
            }
        }

        /// <summary>
        /// Tries to find game {name} from Object-cache.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static bool TryGetGame(string name, out Game game)
        {
            game = Repository<Game>.FindFirstBy(x => x.Name.ToLower() == name.ToLower());

            if (game == null)
                return false;

            return true;
        }

        /// <summary>
        /// Tries to find game {id} from Object-cache.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static bool TryGetGame(long id, out Game game)
        {
            game = Repository<Game>.FindFirstBy(x => x.Id == id);

            if (game == null)
                return false;

            return true;
        }
    }
}
