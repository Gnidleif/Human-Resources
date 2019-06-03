using HumanResources.Utilities;
using Newtonsoft.Json;
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

    public bool Contains(ulong gid, ulong id) => this.List.ContainsKey(gid) && this.List[gid].ContainsKey(id);

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

    public bool Pop(ulong gid, ulong id)
    {
      if (this.Contains(gid, id))
      {
        return this.List[gid].Remove(id);
      }
      return false;
    }

    public bool Pop(ulong gid) => this.List.Remove(gid);

    public bool Push(ulong gid, ulong id)
    {
      if (id == default)
      {
        return false;
      }
      if (!this.List.ContainsKey(gid))
      {
        this.List.Add(gid, new Dictionary<ulong, ReactionHelper>());
      }
      if (!this.List[gid].ContainsKey(id))
      {
        this.List[gid].Add(id, null);
        return true;
      }
      return false;
    }

    public bool Push(ulong gid, ulong id, Regex rgx, string p)
    {
      if (string.IsNullOrWhiteSpace(p) || !this.Push(gid, id))
      {
        return false;
      }
      this.List[gid][id] = new ReactionHelper
      {
        Rgx = rgx,
        Phrases = new List<string>() { p },
      };
      return true;
    }

    public bool Append(ulong gid, ulong id, string p)
    {
      if (!string.IsNullOrWhiteSpace(p) && this.Contains(gid, id))
      {
        this.List[gid][id].Phrases.Add(p);
        return true;
      }
      return false;
    }

    public bool Enable(ulong gid, ulong id, bool state)
    {
      if (this.Contains(gid, id))
      {
        this.List[gid][id].Enabled = state;
        return true;
      }
      return false;
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
        if (obj.Enabled && obj.Rgx.IsMatch(words))
        {
          result.Add(obj.GetRandom(new Random(DateTime.UtcNow.Millisecond)));
        }
      }
      return result;
    }

    public string ToJson(ulong gid, ulong id)
    {
      if (id == default)
      {
        return JsonConvert.SerializeObject(this.List[gid], Formatting.Indented);
      }
      return this.Contains(gid, id) ? JsonConvert.SerializeObject(this.List[gid][id], Formatting.Indented) : string.Empty;
    }
  }

  public class ReactionHelper
  {
    public Regex Rgx { get; set; }
    public List<string> Phrases { get; set; }
    public bool Enabled { get; set; }
    public string GetRandom(Random rand) => this.Phrases[rand.Next(0, this.Phrases.Count)];
  }
}
