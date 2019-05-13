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
            if (File.Exists(this.Path) ? JsonUtil.TryRead(this.Path, out temp) : JsonUtil.TryWrite(this.Path, temp))
            {
                this.List = temp;
            }
        }

        public bool Close()
        {
            var toDelete = this.List.Keys.Where(x => !this.List[x].Any()).ToList();
            toDelete.ForEach(x => this.List.Remove(x));

            return this.Save();
        }

        public bool Contains(ulong gid, ulong uid) => this.List.ContainsKey(gid) && this.List[gid].ContainsKey(uid);

        public bool RemoveGuild(ulong gid) => this.List.Remove(gid);

        public bool Pop(ulong gid, ulong uid)
        {
            if (this.Contains(gid, uid))
            {
                return this.List[gid].Remove(uid);
            }
            return false;
        }

        public bool Push(ulong gid, ulong uid) => this.Push(gid, uid, LogUtil.UnixTime(DateTime.UtcNow.AddMinutes(10)));

        public bool Push(ulong gid, ulong uid, ulong time)
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
