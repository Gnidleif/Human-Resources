using Discord.WebSocket;
using HumanResources.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using HumanResources.AdminModule;
using Discord;

namespace HumanResources
{
  class Program
  {
    private CommandHandler Handler { get; set; }
    private List<IStaticResource> Resources { get; set; }
    private static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

    public async Task MainAsync()
    {
      if (string.IsNullOrEmpty(Config.Bot.Token))
      {
        Console.WriteLine("No token detected, please provide a valid token:");
        var input = Console.ReadLine();
        Config.Bot = new BotConfig
        {
          Token = input,
          TimeFormat = Config.Bot.TimeFormat,
          Guilds = Config.Bot.Guilds,
        };
      }

      Global.Client = new DiscordSocketClient(new DiscordSocketConfig
      {
        LogLevel = Discord.LogSeverity.Verbose,
      });

      Global.Client.GuildMemberUpdated += Client_GuildMemberUpdated;
      //Global.Client.LatencyUpdated += Client_LatencyUpdated;
      Global.Client.Log += Client_Log;
      Global.Client.LeftGuild += Client_LeftGuild;
      Global.Client.JoinedGuild += Client_JoinedGuild;
      Global.Client.Ready += Client_Ready;
      Global.Client.Disconnected += Client_Disconnected;

      try
      {
        await Global.Client.LoginAsync(Discord.TokenType.Bot, Config.Bot.Token);
        await Global.Client.StartAsync();
      }
      catch (Discord.Net.HttpException e)
      {
        Config.Bot = new BotConfig
        {
          Token = "",
          TimeFormat = Config.Bot.TimeFormat,
          Guilds = Config.Bot.Guilds,
        };
        LogUtil.Write("MainAsync", e.Message);
        return;
      }

      this.Resources = new List<IStaticResource>
      {
        MarkResource.Instance,
        BlacklistResource.Instance,
        TimeoutResource.Instance,
      };
      AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

      this.Handler = new CommandHandler();
      await this.Handler.InitializeAsync();

      await Task.Delay(-1);
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

    private async Task Client_Log(Discord.LogMessage arg)
    {
      LogUtil.Write(arg.Source, arg.Message);
      await Task.CompletedTask;
    }

    private async Task Client_LeftGuild(SocketGuild arg)
    {
      _ = Config.Pop(arg.Id);
      this.Resources.ForEach(x => x.Remove(arg.Id));

      await Task.CompletedTask;
    }

    private async Task Client_JoinedGuild(SocketGuild arg)
    {
      if (Config.Push(arg.Id))
      {
        LogUtil.Write("Client_JoinedGuild", $"Successfully joined {arg.Id}");
      }

      await Task.CompletedTask;
    }

    private async Task Client_Ready()
    {
      Global.Client.Guilds.Select(x => x.Id).ToList().ForEach(id => Config.Push(id));

      _ = Config.Save();
      foreach(var r in this.Resources)
      {
        await r.Initialize();
      }
      this.Resources.ForEach(x => x.Save());

      await Task.CompletedTask;
    }

    private async Task Client_Disconnected(Exception arg)
    {
      _ = Config.Save();
      this.Resources.ForEach(x => x.Close());

      await Task.CompletedTask;
    }

    private void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
      _ = Config.Save();
      this.Resources.ForEach(x => x.Close());
    }
  }
}
