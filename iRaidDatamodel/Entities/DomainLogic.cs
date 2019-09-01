using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    public class DomainLogic<T> : IDomainLogic<T> where T : class
    {
        public override int GetHashCode()
        {
            return (int)Id;
        }

        public long Id { get; private set; }

        public bool Save()
        {
            if(this.Id == 0)
            {
                this.Id = Next();
            }

            return Repository<T>.AddOrUpdate(this as T);
        }

        public bool Save(long UserId)
        {
            User user = User.FindFirst(x => x.Id == UserId);

            if (this.Id == 0)
            {
                this.Id = Next();

                if(user != null)
                    AddLog(user, LogAction.Create);
            }
            else
            {
                if (user != null)
                    AddLog(user, LogAction.Modify);
            }

            return AddOrUpdate(this as T);
        }

        public bool Save(User user, string comment = "")
        {
            if (this.Id == 0)
            {
                this.Id = Next();

                AddLog(user, LogAction.Create);
            }
            else
            {
                AddLog(user, LogAction.Modify, comment);
            }

            return AddOrUpdate(this as T);
        }

        public long Next()
        {    
            if (Id == 0)
            {
                return Repository<T>.Next();
            }
            else
            {
                return Id;
            }
        }

        public void AddLog(User user, LogAction action, string Comment = "")
        {
            Log.LogAction<T>(this as T, user, action, Comment);
        }

        public IEnumerable<Log> GetLog()
        {
            T entity = this as T;

            return Repository<Log>.FindBy(x => x.Type == typeof(T).Name && x.EntityId == entity.GetHashCode());
        }

        public IEnumerable<Log> GetLogForType()
        {
            return Repository<Log>.FindBy(x => x.Type == typeof(T).Name);
        }

        public static int Count()
        {
            return Repository<T>.Count();
        }

        public static IEnumerable<T> All()
        {
            return Repository<T>.All();
        }

        public static IEnumerable<T> Find(Func<T, bool> predicate)
        {
            return Repository<T>.FindBy(predicate);
        }

        public static T FindSingle(Func<T, bool> predicate)
        {
            return Repository<T>.FindSingleBy(predicate);
        }

        public static T FindFirst(Func<T, bool> predicate)
        {
            return Repository<T>.FindFirstBy(predicate);
        }

        public static T FindById(long id)
        {
            return Repository<T>.FindFirstBy(x => x.GetHashCode() == id);
        }

        public static bool AddOrUpdate(T entity)
        {
            return Repository<T>.AddOrUpdate(entity);
        }
    }
}
