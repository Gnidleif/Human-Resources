using HumanResources.Utilities;
using HumanResources.AdminModule;
using HumanResources.TwitterModule;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Discord.WebSocket;
using Discord;
using System.Linq;
using HumanResources.ReactionsModule;

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
        Config.Bot.Token = Console.ReadLine();
      }

      Global.Client = new DiscordSocketClient(new DiscordSocketConfig
      {
        LogLevel = LogSeverity.Verbose,
      });

      Global.Client.UserJoined += Client_UserJoined;
      Global.Client.Log += Client_Log;
      Global.Client.LeftGuild += Client_LeftGuild;
      Global.Client.JoinedGuild += Client_JoinedGuild;
      Global.Client.Ready += Client_Ready;
      Global.Client.Disconnected += Client_Disconnected;

      try
      {
        await Global.Client.LoginAsync(TokenType.Bot, Config.Bot.Token);
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
        _ = Config.Save();
        return;
      }

      this.Resources = new List<IStaticResource>
      {
        MarkResource.Instance,
        BlacklistResource.Instance,
        TimeoutResource.Instance,
        TwitterResource.Instance,
        ReactionResource.Instance,
      };
      AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

      this.Handler = new CommandHandler();
      await this.Handler.InitializeAsync();

      await Task.Delay(-1);
    }

    private async Task Client_LatencyUpdated(int arg1, int arg2)
    {
      _ = Config.Save();
      this.Resources.ForEach(x => x.Close());
      await Task.CompletedTask;
    }

    private async Task Client_UserJoined(SocketGuildUser arg)
    {
      var wait = Config.Bot.Guilds[arg.Guild.Id].Welcome;
      if (wait.Enabled)
      {
        var firstRole = arg.Guild.Roles.First(x => x.Position == wait.Rank);
        if (wait.Time > 0)
        {
          await TimeoutResource.Instance.SetTimeout(arg, wait.Time, new List<ulong> { firstRole.Id });
          if (!string.IsNullOrEmpty(wait.Message))
          {
            await arg.SendMessageAsync(wait.Message);
          }
        }
        else
        {
          try
          {
            await arg.AddRoleAsync(firstRole);
          }
          catch (Exception e)
          {
            LogUtil.Write("Client_UserJoined", e.Message);
          }
        }
      }
    }

    private async Task Client_Log(LogMessage arg)
    {
      LogUtil.Write(arg.Source, arg.Message);
      await Task.CompletedTask;
    }

    private async Task Client_LeftGuild(SocketGuild arg)
    {
      _ = Config.Pop(arg.Id);
      this.Resources.ForEach(x => x.Pop(arg.Id));

      await Task.CompletedTask;
    }

    private async Task Client_JoinedGuild(SocketGuild arg)
    {
      _ = Config.Push(arg.Id);
      await Task.CompletedTask;
    }

    private async Task Client_Ready()
    {
      Global.Client.Guilds.Select(x => x.Id).ToList().ForEach(id => Config.Push(id));

      _ = Config.Save();
      foreach(var r in this.Resources)
      {
        await r.Initialize();
        _ = r.Close();
      }

      Global.Client.LatencyUpdated += Client_LatencyUpdated;
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
