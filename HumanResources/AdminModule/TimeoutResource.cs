using HumanResources.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HumanResources.AdminModule
{
    class TimeoutResource : IStaticResource
    {
        private static readonly Lazy<TimeoutResource> lazy = new Lazy<TimeoutResource>(() => new TimeoutResource());
        private const string Name = "timeouts.json";
        private readonly string Path = $"{Global.ResourceFolder}/{Name}";
        private Dictionary<ulong, Dictionary<ulong, ulong>> List { get; set; }

        public static TimeoutResource Instance { get { return lazy.Value; } }

        private TimeoutResource()
        {
            if (!Directory.Exists(Global.ResourceFolder))
            {
                Directory.CreateDirectory(Global.ResourceFolder);
            }
            var temp = new Dictionary<ulong, Dictionary<ulong, ulong>>();
            if (File.Exists(Path) ? JsonUtil.TryRead(this.Path, out temp) : JsonUtil.TryWrite(this.Path, temp))
            {
                this.List = temp;
            }
        }

        public bool Close()
        {
            var toDelete = new List<ulong>();
            foreach(var id in this.List.Keys)
            {
                if (!this.List[id].Any())
                {
                    toDelete.Add(id);
                }
            }
            foreach(var id in toDelete)
            {
                this.List.Remove(id);
            }

            return this.Save();
        }

        public bool Contains(ulong gid, ulong uid) => this.List.ContainsKey(gid) && this.List[gid].ContainsKey(uid);

        public bool PopGuild(ulong gid)
        {
            if (this.List.ContainsKey(gid))
            {
                return this.List.Remove(gid);
            }
            return false;
        }

        public bool PopUser(ulong gid, ulong uid)
        {
            if (this.Contains(gid, uid))
            {
                return this.List[gid].Remove(uid);
            }
            return false;
        }

        public bool PushUser(ulong gid, ulong uid)
        {
            return this.PushUser(gid, uid, LogUtil.UnixTime(DateTime.UtcNow.AddMinutes(10)));
        }

        public bool PushUser(ulong gid, ulong uid, ulong time)
        {
            if (!this.List.ContainsKey(gid))
            {
                this.List.Add(gid, new Dictionary<ulong, ulong>());
            }
            if (!this.List[gid].ContainsKey(uid))
            {
                this.List[gid].Add(uid, time);
                return true;
            }
            return false;
        }

        public bool Save() => JsonUtil.TryWrite(this.Path, this.List);
    }
}
