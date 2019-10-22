using Discord.WebSocket;
using HumanResources.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HumanResources.AnnounceModule
{
  class AnnounceResource : IStaticResource
  {
    private static readonly Lazy<AnnounceResource> lazy = new Lazy<AnnounceResource>(() => new AnnounceResource());
    private readonly string Path = $"{Global.ResourceFolder}/announce.json";
    private Dictionary<ulong, KeyValuePair<ulong, AnnounceInfo>> List { get; set; }

    public static AnnounceResource Instance { get { return lazy.Value; } }

    private AnnounceResource()
    {
      Global.Client.UserJoined += Client_UserJoined;
      Global.Client.UserLeft += Client_UserLeft;
    }

    public async Task Initialize()
    {
      if (!Directory.Exists(Global.ResourceFolder))
      {
        Directory.CreateDirectory(Global.ResourceFolder);
      }
      var temp = new Dictionary<ulong, KeyValuePair<ulong, AnnounceInfo>>();
      if (File.Exists(this.Path) ? JsonUtil.TryRead(this.Path, out temp) : JsonUtil.TryWrite(this.Path, temp))
      {
        this.List = temp;
      }
      await Task.CompletedTask;
    }

    private async Task Client_UserJoined(Discord.WebSocket.SocketGuildUser arg)
    {
      var gid = arg.Guild.Id;
      if (this.List.ContainsKey(gid))
      {
        var ev = this.List[gid].Value.Events["userjoined"];
        var ch = Global.Client.GetChannel(this.List[gid].Key) as SocketTextChannel;
        if (ch != null && ev.Item2 == true)
        {
          await ch.SendMessageAsync(string.Format(ev.Item1, arg.Mention));
        }
      }
    }

    private async Task Client_UserLeft(Discord.WebSocket.SocketGuildUser arg)
    {
      var gid = arg.Guild.Id;
      if (this.List.ContainsKey(gid))
      {
        var ev = this.List[gid].Value.Events["userleft"];
        var ch = Global.Client.GetChannel(this.List[gid].Key) as SocketTextChannel;
        if (ch != null && ev.Item2 == true)
        {
          await ch.SendMessageAsync(string.Format(ev.Item1, arg.Username));
        }
      }
    }

    public bool Close() => this.Save();

    public bool Contains(ulong gid, ulong cid) => this.List.ContainsKey(gid) && this.List[gid].Key == cid;

    public bool Pop(ulong gid, ulong cid) => this.List.Remove(gid);

    public bool Pop(ulong gid) => this.List.Remove(gid);

    public bool Push(ulong gid, ulong cid)
    {
      if (!this.List.ContainsKey(gid))
      {
        this.List.Add(gid, new KeyValuePair<ulong, AnnounceInfo>());
      }
      if (this.List[gid].Key == 0)
      {
        this.List[gid] = new KeyValuePair<ulong, AnnounceInfo>(cid, new AnnounceInfo());
        return true;
      }
      return false;
    }

    public bool Save() => JsonUtil.TryWrite(this.Path, this.List);

    public bool SetChannel(ulong gid, ulong cid)
    {
      if (!this.List.ContainsKey(gid))
      {
        return false;
      }
      var val = this.List[gid].Value;
      this.List.Remove(gid);
      this.List.Add(gid, new KeyValuePair<ulong, AnnounceInfo>(cid, val));
      return true;
    }

    public bool SetState(ulong gid, string key, bool state)
    {
      if (!this.List.ContainsKey(gid) || !this.List[gid].Value.Events.ContainsKey(key))
      {
        return false;
      }
      var msg = this.List[gid].Value.Events[key.ToLower()].Item1;
      this.List[gid].Value.Events[key] = new Tuple<string, bool>(msg, state);
      return true;
    }

    public bool SetMsg(ulong gid, string key, string msg)
    {
      if (!this.List.ContainsKey(gid) || !this.List[gid].Value.Events.ContainsKey(key))
      {
        return false;
      }
      var state = this.List[gid].Value.Events[key.ToLower()].Item2;
      this.List[gid].Value.Events[key] = new Tuple<string, bool>(msg, state);
      return true;
    }

    public Dictionary<string, string> GetAnnouncements(ulong gid)
    {
      if (!this.List.ContainsKey(gid) || this.List[gid].Value.Events.Count == 0)
      {
        return null;
      }
      var result = new Dictionary<string, string>
      {
        { this.List[gid].Key.ToString(), null },
      };
      foreach(var a in this.List[gid].Value.Events)
      {
        result.Add(a.Key, $"**Message**: '{a.Value.Item1}'\n**State**: " + (a.Value.Item2 == true ? "Enabled" : "Disabled"));
      }
      return result;
    }

    private class AnnounceInfo
    {
      public Dictionary<string, Tuple<string, bool>> Events { get; set; } = new Dictionary<string, Tuple<string, bool>>
      {
        { "userjoined", new Tuple<string, bool>(":white_check_mark: {0} just joined the server, welcome!", true) },
        { "userleft", new Tuple<string, bool>(":negative_squared_cross_mark: {0} just left the server, good bye!", true) },
      };
    }
  }
}
