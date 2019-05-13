using HumanResources.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HumanResources.AdminModule
{
    class BlacklistResource : IStaticResource
    {
        private static readonly Lazy<BlacklistResource> lazy = new Lazy<BlacklistResource>(() => new BlacklistResource());
        private const string Name = "blacklist.json";
        private readonly string Path = $"{Global.ResourceFolder}/{Name}";

        public static BlacklistResource Instance { get { return lazy.Value; } }
        public Dictionary<ulong, HashSet<ulong>> List { get; set; }

        private BlacklistResource()
        {
            if (!Directory.Exists(Global.ResourceFolder))
            {
                Directory.CreateDirectory(Global.ResourceFolder);
            }
            var temp = new Dictionary<ulong, HashSet<ulong>>();
            if (File.Exists(Path) ? JsonUtil.TryRead(Path, out temp) : JsonUtil.TryWrite(Path, temp))
            {
                List = temp;
            }
        }

        public bool Save() => JsonUtil.TryWrite(Path, List);

        public bool Close()
        {
            var toDelete = new HashSet<ulong>();
            foreach(var id in List.Keys)
            {
                if (!List[id].Any())
                {
                    toDelete.Add(id);
                }
            }
            foreach(var id in toDelete)
            {
                List.Remove(id);
            }

            return Save();
        }

        public bool Push(ulong gid, ulong uid)
        {
            if (!List.ContainsKey(gid))
            {
                List.Add(gid, new HashSet<ulong>());
            }
            if (!List[gid].Contains(uid))
            {
                List[gid].Add(uid);
                return true;
            }
            return false;
        }

        public bool Pop(ulong gid, ulong uid)
        {
            if (Contains(gid, uid))
            {
                return List[gid].Remove(uid);
            }
            return false;
        }

        public bool Contains(ulong gid, ulong uid) => List.ContainsKey(gid) && List[gid].Contains(uid);

        public bool RemoveGuild(ulong gid)
        {
            if (List.ContainsKey(gid))
            {
                return List.Remove(gid);
            }
            return false;
        }
    }
}
