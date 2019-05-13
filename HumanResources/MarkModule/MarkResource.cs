using Discord;
using HumanResources.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace HumanResources.MarkModule
{
    public sealed class MarkResource : IStaticResource
    {
        private static readonly Lazy<MarkResource> lazy = new Lazy<MarkResource>(() => new MarkResource());
        private const string Name = "marked.json";
        private Timer ValidateTimer;
        private readonly string Path = $"{Global.ResourceFolder}/{Name}";
        private Dictionary<ulong, HashSet<ulong>> List { get; set; }

        public static MarkResource Instance { get { return lazy.Value; } }
        
        private MarkResource()
        {
            if (!Directory.Exists(Global.ResourceFolder))
            {
                Directory.CreateDirectory(Global.ResourceFolder);
            }
            var temp = new Dictionary<ulong, HashSet<ulong>>();
            if (File.Exists(this.Path) ? JsonUtil.TryRead(this.Path, out temp) : JsonUtil.TryWrite(Path, temp))
            {
                this.List = temp;
            }
        }

        public bool Save() => JsonUtil.TryWrite(this.Path, this.List);

        public bool Close()
        {
            var toDelete = new List<ulong>();
            foreach (var id in this.List.Keys)
            {
                if (!this.List[id].Any())
                {
                    toDelete.Add(id);
                }
            }
            foreach (var id in toDelete)
            {
                this.List.Remove(id);
            }

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
            if (Contains(gid, uid))
            {
                return this.List[gid].Remove(uid);
            }
            return false;
        }

        public bool Contains(ulong gid, ulong uid) => List.ContainsKey(gid) && List[gid].Contains(uid);

        public bool RemoveGuild(ulong gid)
        {
            if (this.List.ContainsKey(gid))
            {
                return this.List.Remove(gid);
            }
            return false;
        }

        public async Task Start()
        {
            ValidateTimer = new Timer()
            {
                Interval = 60 * 1000,
                AutoReset = true,
                Enabled = true,
            };
            ValidateTimer.Elapsed += ValidateTimer_Elapsed;
            LogUtil.Write("MarkHandler:Start", "Mark validation started");

            await Task.CompletedTask;
        }

        private void ValidateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MarkAll();
            _ = Save();
        }

        public void MarkAll()
        {
            foreach (var gid in List.Keys)
            {
                var mark = Config.Bot.Guilds[gid].Mark;
                var guild = Global.Client.GetGuild(gid);
                if (guild == null)
                {
                    continue;
                }
                foreach (var uid in List[gid])
                {
                    var user = guild.GetUser(uid);
                    if (user == null)
                    {
                        continue;
                    }
                    _ = CheckSet(user, mark);
                }
            }
        }

        public async Task CheckSet(IGuildUser user, char mark)
        {
            var preferred = $"{mark} {user.Username}";
            var rgx = new Regex($"^[{mark}] {user.Username}$");
            if (string.IsNullOrEmpty(user.Nickname) || !rgx.IsMatch(user.Nickname))
            {
                try
                {
                    await user.ModifyAsync(x => x.Nickname = preferred);
                }
                catch (Discord.Net.HttpException e)
                {
                    LogUtil.Write("MarkHandler:CheckSet", e.Message);
                    _ = Pop(user.GuildId, user.Id);
                }
            }
        }
    }
}
