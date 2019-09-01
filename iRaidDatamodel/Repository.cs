using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Redis;
using System.Diagnostics;

namespace iRaidDatamodel
{

    [AttributeUsage(AttributeTargets.Class)]
    public class LoadInfo : Attribute
    {
        public LoadInfo(bool StoreInDatastore = true, bool CacheOnLoad = true)
        {
            this.StoreInDatastore = StoreInDatastore;
            this.CacheOnLoad = CacheOnLoad;
        }

        public bool StoreInDatastore { get; set; }
        public bool CacheOnLoad { get; set; }
    }

    public class Repository<T> where T : class
    {
        private static ObjectRepository<T> repo = new ObjectRepository<T>();

        /// <summary>
        /// Load {T} into Object-cache from Data Store.
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        public static void LoadIntoCache()
        {
            Stopwatch s = new Stopwatch();
            s.Start();

            repo.LoadIntoCache();

            s.Stop();
            var count = repo.Count();

            Console.WriteLine(" {0,-39} | {1,-7} | {2,6} ms.", typeof(T).Name.ToString(), count,s.ElapsedMilliseconds);
        }

        ///// <summary>
        ///// Ensures that given type exist in Object-cache.
        ///// </summary>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //public static Type EnsureTypeInCache(Type type)
        //{
        //    //return repo<.EnsureTypeInCache(type);
        //}

        /// <summary>
        /// Find First {T} in Object-cache.
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        /// <param name="predicate">linq statement</param>
        /// <returns></returns>
        public static T FindFirstBy(Func<T, bool> predicate)
        {
            return repo.FindFirstBy(predicate);
        }

        /// <summary>
        /// Find Single {T} in Object-cache.
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        /// <param name="predicate">linq statement</param>
        /// <returns></returns>
        public static T FindSingleBy(Func<T, bool> predicate)
        {
            return repo.FindSingleBy(predicate);
        }

        /// <summary>
        /// Tries to update or Add entity to Object-cache and Data Store.
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        /// <param name="predicate">linq expression</param>
        /// <param name="entity">entity</param>
        public static bool AddOrUpdate(T entity)
        {
            return repo.AddOrUpdate(entity);
        }

        /// <summary>
        /// Returns true of item was added, flase if item already exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool Add(T entity)
        {
            if(Contains(entity))
            {
                return false;
            }

            return repo.AddOrUpdate(entity);
        }

        /// <summary>
        /// Delete single {T} from Object-cache and Data Store.
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        /// <param name="entity">class object</param>
        public static void Remove(T entity) 
        {
            repo.Remove(entity);
        }

        /// <summary>
        /// Check if {T} exists in Object-cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool Contains(T entity)
        {
            return repo.Contains(entity);
        }

        /// <summary>
        /// Find List<T>(predicate) in Object-cache.
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        /// <param name="predicate">linq statement</param>
        /// <returns></returns>
        public static IEnumerable<T> FindBy(Func<T, bool> predicate)
        {
            return repo.FindBy(predicate);
        }

        /// <summary>
        /// Get all {T} from Object-cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> All()
        {
            return repo.All();
        }

        /// <summary>
        /// Get Next Sequence for the given {T} Entity from Data Store. 
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        /// <returns>long</returns>
        public static long Next()
        {
            return repo.Next();
        }

        /// <summary>
        /// Number of records in Object-cache.
        /// </summary>
        /// <returns></returns>
        public static int Count()
        {
            return repo.Count();
        }
    }

    public interface IObjectRepository<T> where T : class
    {
        void LoadIntoCache();
        T FindFirstBy(Func<T, bool> predicate);
        T FindSingleBy(Func<T, bool> predicate);
        bool AddOrUpdate(T entity);
        void Remove(T entity);
        IEnumerable<T> FindBy(Func<T, bool> predicate);
        IEnumerable<T> All();
        long Next();
        bool Contains(T Entity);
        int Count();
    }

    public class ObjectRepository<T> : IObjectRepository<T> where T : class
    {
        private static readonly PooledRedisClientManager m = new PooledRedisClientManager();
        private static HashSet<T> _cache = new HashSet<T>();

        private IRedisClientsManager RedisManager { get; set; }
        private HashSet<T> Cache { get; set; }

        public ObjectRepository()
        {
            RedisManager = m;
            Cache = _cache;
        }

        public ObjectRepository(IRedisClientsManager redisManager, HashSet<T> cache)
        {
            RedisManager = redisManager;
            Cache = cache;
        }

        /// <summary>
        /// Load {T} into Object-cache from Data Store.
        /// </summary>
        public void LoadIntoCache()
        {
            // Lets make sure we never replace _cache[T] if key is already present. 
            Cache = new HashSet<T>(RedisGetAll().ToList());
        }
        
        /// <summary>
        /// Find First {T} in Object-cache.
        /// </summary>
        /// <param name="predicate">linq statement</param>
        /// <returns></returns>
        public T FindFirstBy(Func<T, bool> predicate)
        {
            // Lets prevent race conditions, locking down cache.
            lock (Cache)
            {
                return Cache.Where(predicate).FirstOrDefault();
            }
        }

        /// <summary>
        /// Find Single {T} in Object-cache.
        /// </summary>
        /// <param name="predicate">linq statement</param>
        /// <returns></returns>
        public T FindSingleBy(Func<T, bool> predicate)
        {
            // Lets prevent race conditions, locking down cache.
            lock (Cache)
            {
                return Cache.Where(predicate).SingleOrDefault();
            }
        }

        /// <summary>
        /// Tries to update or Add entity to Object-cache and Data Store.
        /// </summary>
        /// <param name="predicate">linq expression</param>
        /// <param name="entity">entity</param>
        public bool AddOrUpdate(T entity)
        {
            // Lets prevent race conditions, locking down cache.
            lock (Cache)
            {
                if (Cache.Contains(entity))
                {
                    Cache.Remove(entity);
                }
                Cache.Add(entity);
            }

            // Redis does not care if record is new or old as it will Add or Update regardless.
            RedisStore(entity);

            return true;
        }

        /// <summary>
        /// Delete single {T} from Object-cache and Data Store.
        /// </summary>
        /// <param name="entity">class object</param>
        public void Remove(T entity)
        {
            // Lets prevent race conditions, locking down cache.
            lock (Cache)
            {
                if (Cache.Contains(entity))
                {
                    Cache.Remove(entity);
                }

                RedisDelete(entity);
            }
        }

        /// <summary>
        /// Check if {T} exists in Object-cache.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Contains(T entity)
        {
            // Lets prevent race conditions, locking down cache.
            lock (Cache)
            {
                return Cache.Contains(entity);
            }
        }

        /// <summary>
        /// Find List<T>(predicate) in Object-cache.
        /// </summary>
        /// <param name="predicate">linq statement</param>
        /// <returns></returns>
        public IEnumerable<T> FindBy(Func<T, bool> predicate)
        {
            // Lets prevent race conditions, locking down cache.
            lock (Cache)
            {
                return Cache.Where(predicate);
            }
        }

        /// <summary>
        /// Get all {T} from Object-cache.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> All()
        {
            // Lets prevent race conditions, locking down cache.
            lock (Cache)
            {
                return Cache;
            }
        }

        /// <summary>
        /// Get Next Sequence for the given {T} Entity from Data Store. 
        /// </summary>
        /// <returns>long</returns>
        public long Next()
        {
            return RedisNext();
        }

        /// <summary>
        /// Number of records stored in object-cache.
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            lock (Cache)
            {
                return Cache.Count;
            }
        }

        #region Redis Commands
        //
        // Following methods are ment as static private methods.
        //

        private long RedisNext()
        {
            using (var ctx = RedisManager.GetClient())
                return ctx.As<T>().GetNextSequence();
        }

        private void RedisDelete(T entity)
        {
            using (var ctx = RedisManager.GetClient())
                ctx.As<T>().Delete(entity);
        }

        private T RedisFind(long id)
        {
            using (var ctx = RedisManager.GetClient())
                return ctx.As<T>().GetById(id);
        }

        private HashSet<T> RedisGetAll() 
        {
            using (var ctx = RedisManager.GetClient())
                return new HashSet<T>(ctx.As<T>().GetAll());
        }

        private void RedisStore(T entity)
        {
            using (var ctx = RedisManager.GetClient())
                ctx.Store<T>(entity);
        }

        #endregion
    }
}
