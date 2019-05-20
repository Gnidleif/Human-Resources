using Discord;
using Discord.WebSocket;
using HumanResources.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HumanResources.AdminModule
{
  public sealed class MarkResource : IStaticResource
  {
    private static readonly Lazy<MarkResource> lazy = new Lazy<MarkResource>(() => new MarkResource());
    private const string Name = "marked.json";
    private readonly string Path = $"{Global.ResourceFolder}/{Name}";
    private Dictionary<ulong, HashSet<ulong>> List { get; set; }

    public static MarkResource Instance { get { return lazy.Value; } }

    private MarkResource()
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
        foreach (var gid in this.List.Keys)
        {
          var mark = Config.Bot.Guilds[gid].Mark;
          var guild = Global.Client.GetGuild(gid);
          if (guild == null)
          {
            continue;
          }
          foreach (var uid in this.List[gid])
          {
            var user = guild.GetUser(uid);
            if (user == null)
            {
              continue;
            }
            await this.CheckSet(user, mark);
          }
        }
      }

      Global.Client.GuildMemberUpdated += Client_GuildMemberUpdated;
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

    public bool Contains(ulong gid) => this.List.ContainsKey(gid);

    public bool Contains(ulong gid, ulong uid) => this.Contains(gid) && this.List[gid].Contains(uid);

    public bool Remove(ulong gid) => this.List.Remove(gid);

    public async Task CheckSet(IGuildUser user, char mark)
    {
      var rgx = new Regex($"^[{mark}] {user.Username}$");
      if (string.IsNullOrEmpty(user.Nickname) || !rgx.IsMatch(user.Nickname))
      {
        var preferred = $"{mark} {user.Username}";
        try
        {
          await user.ModifyAsync(x => x.Nickname = preferred);
        }
        catch (Discord.Net.HttpException e)
        {
          LogUtil.Write("MarkHandler:CheckSet", e.Message);
          _ = this.Pop(user.GuildId, user.Id);
        }
      }
    }

    public async Task CheckSetGuild(IGuild guild)
    {
      if (this.Contains(guild.Id))
      {
        var g = guild as SocketGuild;
        foreach(var user in g.Users)
        {
          if (this.Contains(g.Id, user.Id))
          {
            await this.CheckSet(user, Config.Bot.Guilds[g.Id].Mark);
          }
        }
      }
    }

    private async Task Client_GuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
    {
      if (string.Compare(arg1.Nickname, arg2.Nickname) == 0)
      {
        return;
      }
      var user = arg2 as IGuildUser;
      if (MarkResource.Instance.Contains(user.GuildId, user.Id))
      {
        await MarkResource.Instance.CheckSet(user, Config.Bot.Guilds[user.GuildId].Mark);
      }
    }
  }
}
