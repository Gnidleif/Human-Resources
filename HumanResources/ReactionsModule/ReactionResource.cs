using HumanResources.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HumanResources.ReactionsModule
{
  public class ReactionResource : IStaticResource
  {
    private static readonly Lazy<ReactionResource> lazy = new Lazy<ReactionResource>(() => new ReactionResource());
    private readonly string Path = $"{Global.ResourceFolder}/reactions.json";
    private Dictionary<ulong, Dictionary<ulong, ReactionHelper>> List { get; set; }

    public static ReactionResource Instance { get { return lazy.Value; } }

    private ReactionResource()
    {
    }

    public bool Close()
    {
      var toDelete = this.List.Keys.Where(x => !this.List[x].Any()).ToList();
      toDelete.ForEach(x => this.List.Remove(x));

      return this.Save();
    }

    public bool Contains(ulong gid, ulong hash) => this.List.ContainsKey(gid) && this.List[gid].ContainsKey(hash);

    public async Task Initialize()
    {
      if (!Directory.Exists(Global.ResourceFolder))
      {
        Directory.CreateDirectory(Global.ResourceFolder);
      }
      var temp = new Dictionary<ulong, Dictionary<ulong, ReactionHelper>>();
      if (File.Exists(this.Path) ? JsonUtil.TryRead(this.Path, out temp) : JsonUtil.TryWrite(this.Path, temp))
      {
        this.List = temp;
      }
      await Task.CompletedTask;
    }

    public bool Pop(ulong gid, ulong hash)
    {
      if (this.Contains(gid, hash))
      {
        return this.List[gid].Remove(hash);
      }
      return false;
    }

    public bool Pop(ulong gid) => this.List.Remove(gid);

    public bool Push(ulong gid, ulong hash)
    {
      if (!this.List.ContainsKey(gid))
      {
        this.List.Add(gid, new Dictionary<ulong, ReactionHelper>());
      }
      if (!this.List[gid].ContainsKey(hash))
      {
        this.List[gid].Add(hash, null);
        return true;
      }
      return false;
    }

    public bool Push(ulong gid, Regex rgx, string p)
    {
      var hash = (ulong)rgx.ToString().GetHashCode();
      if (!this.Push(gid, hash))
      {
        return false;
      }
      this.List[gid][hash] = new ReactionHelper
      {
        Rgx = rgx,
        Phrase = p,
      };
      return true;
    }

    public bool Save() => JsonUtil.TryWrite(this.Path, this.List);

    public List<string> Find(ulong gid, string words)
    {
      var result = new List<string>();
      if (!this.List.ContainsKey(gid))
      {
        return result;
      }
      foreach(var obj in this.List[gid].Values)
      {
        if (obj.Rgx.IsMatch(words))
        {
          result.Add(obj.Phrase);
        }
      }
      return result;
    }

    public List<ReactionHelper> GetGuild(ulong gid) => this.List[gid]?.Values.ToList();
  }

  public class ReactionHelper
  {
    public Regex Rgx { get; set; }
    public string Phrase { get; set; }
  }
}
