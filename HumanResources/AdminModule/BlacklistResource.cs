using HumanResources.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HumanResources.AdminModule
{
  class BlacklistResource : IStaticResource
  {
    private static readonly Lazy<BlacklistResource> lazy = new Lazy<BlacklistResource>(() => new BlacklistResource());
    private readonly string Path = $"{Global.ResourceFolder}/blacklist.json";
    private Dictionary<ulong, HashSet<ulong>> List { get; set; }

    public static BlacklistResource Instance { get { return lazy.Value; } }
    
    private BlacklistResource()
    {
    }

    public async Task Initialize()
    {
      if (!Directory.Exists(Global.ResourceFolder))
      {
        Directory.CreateDirectory(Global.ResourceFolder);
      }
      var temp = new Dictionary<ulong, HashSet<ulong>>();
      if (File.Exists(this.Path) ? JsonUtil.TryRead(this.Path, out temp) : JsonUtil.TryWrite(this.Path, temp))
      {
        this.List = temp;
      }
      await Task.CompletedTask;
    }

    public bool Save() => JsonUtil.TryWrite(this.Path, this.List);

    public bool Close()
    {
      var toDelete = this.List.Keys.Where(x => !this.List[x].Any()).ToList();
      toDelete.ForEach(x => this.List.Remove(x));

      return this.Save();
    }

    public bool Push(ulong gid, ulong uid)
    {
      if (!this.List.ContainsKey(gid))
      {
        this.List.Add(gid, new HashSet<ulong>());
      }
      if (!this.List[gid].Contains(uid))
      {
        this.List[gid].Add(uid);
        return true;
      }
      return false;
    }

    public bool Pop(ulong gid, ulong uid)
    {
      if (this.Contains(gid, uid))
      {
        return this.List[gid].Remove(uid);
      }
      return false;
    }

    public bool Contains(ulong gid, ulong uid) => this.List.ContainsKey(gid) && this.List[gid].Contains(uid);

    public bool Pop(ulong gid) => this.List.Remove(gid);
  }
}
