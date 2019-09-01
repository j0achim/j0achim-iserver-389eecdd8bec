using System;
namespace iRaidDatamodel.Entities
{
    interface IDomainLogic<T>
     where T : class
    {
        void AddLog(User user, LogAction action, string Comment = "");
        int GetHashCode();
        System.Collections.Generic.IEnumerable<Log> GetLogForType();
        System.Collections.Generic.IEnumerable<Log> GetLog();
        long Id { get; }
        long Next();
        bool Save();
        bool Save(long UserId);
        bool Save(User user, string comment = "");
    }
}
