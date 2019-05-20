using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HumanResources.AdminModule;
using HumanResources.Utilities;
using System.Threading.Tasks;

namespace HumanResources.Settings
{
  [Group("settings")]
  [Remarks("Most of these functions are only accessible by guild administrators")]
  public class Settings : ModuleBase<SocketCommandContext>
  {
    [Command, Summary("Returns the bot settings for the guild")]
    public async Task GetSettings()
    {
      var cfg = Config.Bot.Guilds[Context.Guild.Id];
      var user = Context.User as SocketGuildUser;

      var embed = new EmbedBuilder();
      embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl());
      embed.AddField("Prefix", $"**{cfg.Prefix}**", true);
      embed.AddField("Mark", $"**{cfg.Mark}**", true);
      embed.AddField("Blacklist on mark?", $"**{cfg.MarkList}**");
      embed.WithFooter(LogUtil.LogTime);

      await ReplyAsync("", false, embed.Build());
    }

    [Command("prefix"), Summary("Set command prefix for the guild")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task SetPrefix([Summary("The new prefix")] char prefix)
    {
      Config.Bot.Guilds[Context.Guild.Id].Prefix = prefix;
      await ReplyAsync($":white_check_mark: Successfully set prefix to **{prefix}**");
    }

    [Command("mark"), Summary("Set mark for the guild")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task SetMark([Summary("The new mark")] char mark)
    {
      Config.Bot.Guilds[Context.Guild.Id].Mark = mark;
      await ReplyAsync($":white_check_mark: Successfully set mark to **{mark}**");
      await MarkResource.Instance.CheckSetGuild(Context.Guild);
    }

    [Command("marklist"), Summary("Set if marked members are also blacklisted or not")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task SetMarkList([Summary("True means all marked marked members are blacklisted while marked")] bool state)
    {
      Config.Bot.Guilds[Context.Guild.Id].MarkList = state;
      await ReplyAsync($":white_check_mark: Successfully set blacklist on mark to **{state}**");
    }
  }
}
