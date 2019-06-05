using System.Collections.Generic;
using System.IO;
using HumanResources.Utilities;

namespace HumanResources
{
  class Config
  {
    private const string Name = "config.json";
    private static readonly string Path = $"{Global.ResourceFolder}/{Name}";

    public static BotConfig Bot { get; set; }

    static Config()
    {
      if (!Directory.Exists(Global.ResourceFolder))
      {
        Directory.CreateDirectory(Global.ResourceFolder);
      }

      var temp = new BotConfig();
      if (File.Exists(Path) ? JsonUtil.TryRead(Path, out temp) : JsonUtil.TryWrite(Path, temp))
      {
        Bot = temp;
      }
    }

    public static bool Save() => JsonUtil.TryWrite(Path, Bot);

    public static bool Push(ulong id)
    {
      if (!Bot.Guilds.ContainsKey(id))
      {
        Bot.Guilds.Add(id, new GuildConfig());
        return true;
      }
      return false;
    }

    public static bool Pop(ulong id) => Bot.Guilds.Remove(id);
  }

  public class BotConfig
  {
    public string Token { get; set; }
    public string TimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
    public Dictionary<ulong, GuildConfig> Guilds { get; set; } = new Dictionary<ulong, GuildConfig>();
  }

  public class GuildConfig
  {
    public char Prefix { get; set; } = '!';
    public char Mark { get; set; } = '⭐';
    public bool MarkList { get; set; } = false;
    public WelcomeConfig Welcome { get; set; } = new WelcomeConfig();
    public MarkovConfig Markov { get; set; } = new MarkovConfig();
  }

  public class WelcomeConfig
  {
    public bool Enabled { get; set; } = false;
    public uint Time { get; set; } = 10;
    public int Rank { get; set; } = 1;
    public string Message { get; set; } = "Welcome! You'll gain full privileges soon.";
  }

  public class MarkovConfig
  {
    public uint Step { get; set; } = 1;
    public uint Count { get; set; } = 20;
    public uint Source { get; set; } = 500;
    public uint Chance { get; set; } = 5;
  }
}
