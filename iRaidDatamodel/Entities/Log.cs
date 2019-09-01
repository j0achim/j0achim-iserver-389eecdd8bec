using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class Log : DomainLogic<Log>
    {
        public Log()
        {
            this.Time = DateTime.UtcNow;
        }

        public string Type { get; set; }
        public long EntityId { get; set; }
        public long UserId { get; set; }
        public DateTime Time { get; set; }
        public LogAction Action { get; set; }
        public string Comment { get; set; }

        /// <summary>
        /// Log an action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="user"></param>
        /// <param name="action"></param>
        public static void LogAction<T>(T entity, User user, LogAction action, string comment = "") where T : class
        {
            Log log = new Log();
            log.Type = typeof(T).Name;
            log.EntityId = entity.GetHashCode();
            log.Action = action;
            log.UserId = user.Id;
            log.Comment = comment;
            log.Save();
        }

        /// <summary>
        /// Get lost of log transactions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static IEnumerable<Log> GetLog<T>(T entity) where T : class
        {
            return Repository<Log>.FindBy(x => x.Type == typeof(T).Name && x.EntityId == entity.GetHashCode()).OrderByDescending(x => x.Time);
        }

        public User User()
        {
            return Repository<User>.FindSingleBy(x => x.Id == UserId);
        }
    }

    public enum LogAction
    {
        Create = 1,
        Modify = 2,
        Delete = 3,
        AuthSuccess = 4,
        AuthFailed = 5
    }
}
