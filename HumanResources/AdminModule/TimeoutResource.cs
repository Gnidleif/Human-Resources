using Discord;
using HumanResources.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace HumanResources.AdminModule
{
  class TimeoutResource : IStaticResource
  {
    private static readonly Lazy<TimeoutResource> lazy = new Lazy<TimeoutResource>(() => new TimeoutResource());
    private const string Name = "timeouts.json";
    private readonly string Path = $"{Global.ResourceFolder}/{Name}";
    private Dictionary<ulong, Dictionary<ulong, TimeoutMember>> List { get; set; }

    public static TimeoutResource Instance { get { return lazy.Value; } }

    private TimeoutResource()
    {
    }

    public async Task Initialize()
    {
      if (!Directory.Exists(Global.ResourceFolder))
      {
        Directory.CreateDirectory(Global.ResourceFolder);
      }
      var temp = new Dictionary<ulong, Dictionary<ulong, TimeoutMember>>();
      if (File.Exists(this.Path) ? JsonUtil.TryRead(this.Path, out temp) : JsonUtil.TryWrite(this.Path, temp))
      {
        this.List = new Dictionary<ulong, Dictionary<ulong, TimeoutMember>>();
        foreach (var gid in temp.Keys)
        {
          foreach (var uid in temp[gid].Keys)
          {
            if (this.Push(gid, uid))
            {
              this.List[gid][uid] = new TimeoutMember();
              this.List[gid][uid].RoleIds.AddRange(temp[gid][uid].RoleIds);
              var tick = this.MakeTimer(gid, uid, temp[gid][uid].Time);
              if (tick == null)
              {
                this.UnknownUnset(gid, uid);
              }
              else
              {
                this.List[gid][uid].Time = temp[gid][uid].Time;
                this.List[gid][uid].Tick = tick;
              }
            }
          }
        }
      }
      await Task.CompletedTask;
    }

    public bool Close()
    {
      var toDelete = this.List.Keys.Where(x => !this.List[x].Any()).ToList();
      toDelete.ForEach(x => this.List.Remove(x));

      return this.Save();
    }

    public bool Contains(ulong gid, ulong uid) => this.List.ContainsKey(gid) && this.List[gid].ContainsKey(uid);

    public bool Remove(ulong gid) => this.List.Remove(gid);

    public bool Pop(ulong gid, ulong uid)
    {
      if (this.Contains(gid, uid))
      {
        return this.List[gid].Remove(uid);
      }
      return false;
    }

    public bool Push(ulong gid, ulong uid)
    {
      if (!this.List.ContainsKey(gid))
      {
        this.List.Add(gid, new Dictionary<ulong, TimeoutMember>());
      }
      if (!this.List[gid].ContainsKey(uid))
      {
        this.List[gid].Add(uid, null);
        return true;
      }
      return false;
    }

    public bool Save() => JsonUtil.TryWrite(this.Path, this.List);

    private async Task UnknownUnset(ulong gid, ulong uid)
    {
      var g = Global.Client.GetGuild(gid);
      var u = g?.GetUser(uid);
      if (g == null)
      {
        this.Remove(gid);
      }
      else if (u == null)
      {
        this.Pop(gid, uid);
      }
      else
      {
        await this.UnsetTimeout(u);
      }
    }

    private Timer MakeTimer(ulong gid, ulong uid, DateTime time)
    {
      var diff = (time - DateTime.Now).TotalMilliseconds;
      if (diff < 0)
      {
        return null;
      }
      var tick = new Timer
      {
        Interval = diff,
        AutoReset = false,
        Enabled = true,
      };
      tick.Elapsed += (object sender, ElapsedEventArgs e) =>
      {
        _ = this.UnknownUnset(gid, uid);
      };
      return tick;
    }

    public async Task SetTimeout(IGuildUser user, uint minutes)
    {
      var time = DateTime.Now.AddMinutes(minutes);
      var roleIds = user.RoleIds.ToList();
      var gid = user.GuildId;
      var uid = user.Id;
      if (!this.Push(gid, uid))
      {
        roleIds.AddRange(this.List[gid][uid].RoleIds);
        roleIds = roleIds.Distinct().ToList();
      }
      this.List[gid][uid] = new TimeoutMember
      {
        RoleIds = roleIds,
        Time = time,
        Tick = this.MakeTimer(gid, uid, time),
      };
      if (this.List[gid][uid].Tick == null)
      {
        try
        {
          await this.UnsetTimeout(user);
        }
        catch (Exception e)
        {
          throw e;
        }
        return;
      }

      try
      {
        var roles = roleIds
            .Select(x => user.Guild.GetRole(x))
            .Where(x => !x.IsManaged && x != user.Guild.EveryoneRole)
            .ToList();
        await user.RemoveRolesAsync(roles);
      }
      catch (Exception e)
      {
        LogUtil.Write("TimeoutResource:SetTimeout", e.Message);
        throw e;
      }
    }

    public async Task UnsetTimeout(IGuildUser user)
    {
      if (!this.Contains(user.GuildId, user.Id))
      {
        return;
      }
      try
      {
        var roles = this.List[user.GuildId][user.Id].RoleIds
            .Select(x => user.Guild.GetRole(x))
            .Where(x => !x.IsManaged && x != user.Guild.EveryoneRole)
            .ToList();
        await user.AddRolesAsync(roles);
      }
      catch (Exception e)
      {
        LogUtil.Write("TimeoutResource:UnsetTimeout", e.Message);
        throw e;
      }
    }
  }

  public class TimeoutMember
  {
    public List<ulong> RoleIds = new List<ulong>();
    public DateTime Time;
    [JsonIgnore]
    public Timer Tick;
  }
}
