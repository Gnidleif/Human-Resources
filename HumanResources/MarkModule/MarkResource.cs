﻿using Discord;
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

        public static MarkResource Instance { get { return lazy.Value; } }

        public Dictionary<ulong, HashSet<ulong>> List { get; set; }

        private MarkResource()
        {
            if (!Directory.Exists(Global.ResourceFolder))
            {
                Directory.CreateDirectory(Global.ResourceFolder);
            }
            var temp = new Dictionary<ulong, HashSet<ulong>>();
            if (File.Exists(Path) ? JsonUtil.TryRead(Path, out temp) : JsonUtil.TryWrite(Path, temp))
            {
                List = temp;
            }
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
                    _ = PopUser(user.GuildId, user.Id);
                }
            }
        }

        public bool Save() => JsonUtil.TryWrite(Path, List);

        public bool Close()
        {
            var toDelete = new HashSet<ulong>();
            foreach (var id in List.Keys)
            {
                if (!List[id].Any())
                {
                    toDelete.Add(id);
                }
            }
            foreach (var id in toDelete)
            {
                List.Remove(id);
            }

            return Save();
        }

        public bool PushUser(ulong gid, ulong uid)
        {
            if (!List.ContainsKey(gid))
            {
                List.Add(gid, new HashSet<ulong>());
            }
            if (!List[gid].Contains(uid))
            {
                List[gid].Add(uid);
                return true;
            }
            return false;
        }

        public bool PopUser(ulong gid, ulong uid)
        {
            if (Contains(gid, uid))
            {
                return List[gid].Remove(uid);
            }
            return false;
        }

        public bool Contains(ulong gid, ulong uid) => List.ContainsKey(gid) && List[gid].Contains(uid);

        public bool PopGuild(ulong gid)
        {
            if (List.ContainsKey(gid))
            {
                List.Remove(gid);
                return true;
            }
            return false;
        }
    }
}
