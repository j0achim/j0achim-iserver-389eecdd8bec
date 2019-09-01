using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    public enum AccessLevel
    {
        // Banned or Revoked Access Level.
        None = 0,
        // No access required.
        Pub = 1,
        // Guest, default permission level execpt one given specifically.
        Other = 2,
        // Member Access Level.
        Member = 4,
        // Leader Access Level.
        Leader = 8,
        // Admin Access Level.
        Admin = 16,
        // SuperAdmin typically for configuration specific tasks.
        SuperAdmin = 32,
    }
}
