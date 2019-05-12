using System.Collections.Generic;
using System.IO;
using HumanResources.Utilities;

namespace HumanResources
{
    class Config
    {
        private const string Name = "config.json";
        private static readonly string Path = $"{Global.ResourceFolder}/{Name}";

        public static GuildConfig DefaultGuildConfig
        {
            get
            {
                return new GuildConfig
                {
                    Prefix = '!',
                    Mark = '⭐',
                };
            }
        }

        public static BotConfig Bot { get; set; }
        static Config()
        {
            if (!Directory.Exists(Global.ResourceFolder))
            {
                Directory.CreateDirectory(Global.ResourceFolder);
            }
            var temp = new BotConfig
            {
                TimeFormat = "yyyy-MM-dd HH:mm:ss",
            };
            if (File.Exists(Path) ? JsonUtil.TryRead(Path, out temp) : JsonUtil.TryWrite(Path, temp))
            {
                Bot = temp;
                if (Bot.Guilds == null)
                {
                    Bot = new BotConfig
                    {
                        Token = Bot.Token,
                        TimeFormat = Bot.TimeFormat,
                        Guilds = new Dictionary<ulong, GuildConfig>(),
                    };
                }
            }
        }

        public static bool Save() => JsonUtil.TryWrite(Path, Bot);

        public static bool AddGuild(ulong id)
        {
            if (!Bot.Guilds.ContainsKey(id))
            {
                Bot.Guilds.Add(id, DefaultGuildConfig);
                return true;
            }
            return false;
        }

        public static bool DeleteGuild(ulong id) => Bot.Guilds.Remove(id);

        public static bool UpdateGuild(ulong id, GuildConfig newCfg)
        {
            if (Bot.Guilds.ContainsKey(id))
            {
                Bot.Guilds[id] = newCfg;
                return true;
            }
            return false;
        }

        public static bool ResetGuild(ulong id)
        {
            if (Bot.Guilds.ContainsKey(id))
            {
                Bot.Guilds[id] = DefaultGuildConfig;
                return true;
            }
            return false;
        }
    }

    public struct BotConfig
    {
        public string Token;
        public string TimeFormat;
        public Dictionary<ulong, GuildConfig> Guilds;
    }

    public struct GuildConfig
    {
        public char Prefix;
        public char Mark;
    }
}
