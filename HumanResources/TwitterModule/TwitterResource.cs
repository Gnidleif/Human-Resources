using Discord;
using HumanResources.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HumanResources.TwitterModule
{
  public class TwitterResource : IStaticResource
  {
    private static readonly Lazy<TwitterResource> lazy = new Lazy<TwitterResource>(() => new TwitterResource());
    private const string Name = "twitter.json";
    private readonly string Path = $"{Global.ResourceFolder}/{Name}";
    private TwitterConfig Config { get; set; }

    public static TwitterResource Instance { get { return lazy.Value; } }

    private TwitterResource()
    {
    }

    public bool Close()
    {
      var toDelete = this.Config.Follows.Keys.Where(x => !this.Config.Follows[x].Any()).ToList();
      toDelete.ForEach(x => this.Config.Follows.Remove(x));

      return this.Save();
    }

    public bool Contains(ulong gid)
    {
      return this.Config.Follows.ContainsKey(gid);
    }

    public bool Contains(ulong gid, ulong cid)
    {
      return this.Contains(gid) && this.Config.Follows[gid].ContainsKey(cid);
    }

    public bool Contains(ulong gid, ulong cid, ulong uid)
    {
      return this.Contains(gid, cid) && this.Config.Follows[gid][cid].Users.Contains(uid);
    }

    public async Task Initialize()
    {
      if (!Directory.Exists(Global.ResourceFolder))
      {
        Directory.CreateDirectory(Global.ResourceFolder);
      }
      var temp = new TwitterConfig();
      if (File.Exists(Path) ? JsonUtil.TryRead(Path, out temp) : JsonUtil.TryWrite(Path, temp))
      {
        this.Config = new TwitterConfig();
        foreach (var gid in temp.Follows.Keys)
        {
          foreach (var cid in temp.Follows[gid].Keys)
          {
            if (this.Push(gid, cid))
            {
              this.Config.Follows[gid][cid].Users = temp.Follows[gid][cid].Users;
            }
          }
        }
      }
      await Task.CompletedTask;
    }

    public bool Pop(ulong gid)
    {
      if (this.Contains(gid))
      {
        return this.Config.Follows.Remove(gid);
      }
      return false;
    }

    public bool Pop(ulong gid, ulong cid)
    {
      if (this.Contains(gid, cid))
      {
        return this.Config.Follows[gid].Remove(cid);
      }
      return false;
    }

    public bool Pop(ulong gid, ulong cid, ulong uid)
    {
      if (this.Contains(gid, cid, uid))
      {
        return this.Config.Follows[gid][cid].Users.Remove(uid);
      }
      return false;
    }

    public bool Push(ulong gid, ulong cid)
    {
      if (!this.Config.Follows.ContainsKey(gid))
      {
        this.Config.Follows.Add(gid, new Dictionary<ulong, ChannelUsers>());
      }
      if (!this.Config.Follows[gid].ContainsKey(cid))
      {
        var g = Global.Client.GetGuild(gid);
        if (g == null)
        {
          _ = this.Pop(gid);
          return false;
        }
        var c = g.GetChannel(cid);
        if (c == null)
        {
          _ = this.Pop(gid, cid);
          return false;
        }
        this.Config.Follows[gid].Add(cid, new ChannelUsers
        {
          Channel = c as IMessageChannel,
        });
        return true;
      }
      return false;
    }

    public bool Push(ulong gid, ulong cid, ulong uid)
    {
      if (!this.Contains(gid, cid))
      {
        _ = this.Push(gid, cid);
      }
      return this.Config.Follows[gid][cid].Users.Add(uid);
    }

    public bool Save() => JsonUtil.TryWrite(this.Path, this.Config);
  }

  public class TwitterConfig
  {
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string AccessTokenSecret { get; set; } = string.Empty;
    public Dictionary<ulong, Dictionary<ulong, ChannelUsers>> Follows { get; set; } = new Dictionary<ulong, Dictionary<ulong, ChannelUsers>>();
  }

  public class ChannelUsers
  {
    public HashSet<ulong> Users { get; set; } = new HashSet<ulong>();
    [JsonIgnore]
    public IMessageChannel Channel { get; set; }
  }
}
