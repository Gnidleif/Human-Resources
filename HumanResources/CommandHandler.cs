using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HumanResources.AdminModule;
using HumanResources.Utilities;
using System.Reflection;
using System.Threading.Tasks;

namespace HumanResources
{
  class CommandHandler
  {
    private CommandService Service { get; set; }

    public async Task InitializeAsync()
    {
      this.Service = new CommandService(new CommandServiceConfig
      {
        CaseSensitiveCommands = false,
        DefaultRunMode = RunMode.Async,
        LogLevel = Discord.LogSeverity.Verbose,
      });
      await this.Service.AddModulesAsync(Assembly.GetEntryAssembly(), null);

      Global.Client.MessageReceived += Client_MessageReceived;
    }

    private async Task Client_MessageReceived(SocketMessage arg)
    {
      var msg = arg as SocketUserMessage;
      if (msg == null)
      {
        return;
      }

      var ctx = new SocketCommandContext(Global.Client, msg);
      if (ctx.User.IsBot || MarkResource.Instance.Contains(ctx.Guild.Id, ctx.User.Id) || BlacklistResource.Instance.Contains(ctx.Guild.Id, ctx.User.Id))
      {
        return;
      }

      int argPos = 0;
      char prefix = Config.Bot.Guilds[ctx.Guild.Id].Prefix;
      if (msg.HasCharPrefix(prefix, ref argPos) || msg.HasMentionPrefix(Global.Client.CurrentUser, ref argPos))
      {
        var result = await this.Service.ExecuteAsync(ctx, argPos, null);
        if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
        {
          LogUtil.Write("Client_MessageReceived", $"Message: {msg.Content} | Error: {result.ErrorReason}");
          await ctx.User.SendMessageAsync(result.ErrorReason);
        }
      }
    }
  }
}
