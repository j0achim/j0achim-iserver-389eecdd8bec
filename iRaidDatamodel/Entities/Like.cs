using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class Like : DomainLogic<Like>
    {
        public Like(Type type, long id, User user, bool Liked)
        {
            this.EntityId = id;
            this.UserId = user.Id;
            this.Time = DateTime.UtcNow;
            this.Liked = Liked;
            this.Type = type.Name;
        }

        public long EntityId { get; private set; }
        public long UserId { get; private set; }
        public string Type { get; private set; }
        public DateTime Time { get; private set; }
        public bool Liked { get; private set; }

        public void Rate(bool like)
        {
            this.Liked = like;
        }

        public static bool GetByUserId(Type type, long entity, User user, out Like like)
        {
            like = Repository<Like>.FindFirstBy(x => x.Type == type.Name && x.EntityId == entity && x.UserId == user.Id);

            if (like == null)
                return false;

            return true;
        }

        public static Rating GetRating(Type type, long id)
        {
            var r = Repository<Like>.FindBy(x => x.Type == type.Name && x.EntityId == id);

            // Check if null or zero.
            if (r == null || r.Count() == 0)
                return new Rating(0, 0);

            // Get numbers.
            int total = r.Count();
            int likes = r.Where(x => x.Liked).Count();

            return new Rating(total, likes);
        }
    }

    public class Rating
    {
        public Rating(int total, int likes)
        {
            this.Likes = likes;
            this.Total = total;
            if (total > 0)
            {
                this.Rated = (likes / total) * 100;
            }
            else
            {
                this.Rated = 0;
            }
        }

        public int Likes { get; set; }
        public int Total { get; set; }
        public double Rated { get; set; }
    }
}
