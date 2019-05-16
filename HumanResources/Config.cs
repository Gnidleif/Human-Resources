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

    public static bool Update(ulong id, GuildConfig newCfg)
    {
      if (Bot.Guilds.ContainsKey(id))
      {
        Bot.Guilds[id] = newCfg;
        return true;
      }
      return false;
    }
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
    public bool Marklist { get; set; } = false;
  }
}
