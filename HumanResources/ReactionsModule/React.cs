using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HumanResources.Utilities;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HumanResources.ReactionsModule
{
  [RequireContext(ContextType.Guild)]
  [RequireUserPermission(GuildPermission.Administrator)]
  [Group("react"), Alias("r"), Summary("Collection of functions used to make the bot react to certain phrases")]
  public class React : ModuleBase<SocketCommandContext>
  {
    [Command, Summary("Retrieve regex and phrases for available reactions")]
    public async Task GetReactions(ulong id = default)
    {
      var json = ReactionResource.Instance.ToJson(Context.Guild.Id, id);
      if (string.IsNullOrEmpty(json))
      {
        await Context.User.SendMessageAsync($"No results found");
        return;
      }
      var embed = new EmbedBuilder();
      var user = Context.User as SocketGuildUser;
      embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
      embed.WithDescription($"```json\n{json}\n```");
      embed.WithFooter(LogUtil.LogTime);
      await ReplyAsync("", false, embed.Build());
    }

    [Command("add"), Alias("a"), Summary("Add a reaction to the guild")]
    public async Task AddReaction(ulong id, Regex rgx, [Remainder] string phrase)
    {
      if (ReactionResource.Instance.Push(Context.Guild.Id, id, rgx, phrase))
      {
        await ReplyAsync($":white_check_mark: Successfully added '{id}'");
      }
      else
      {
        await Context.User.SendMessageAsync($"Unable to add duplicate of '{id}'");
      }
    }

    [Command("phrase"), Alias("p"), Summary("Add a phrase to already existing ID")]
    public async Task AddPhrase(ulong id, [Remainder] string phrase)
    {
      if (ReactionResource.Instance.Append(Context.Guild.Id, id, phrase))
      {
        await ReplyAsync($":white_check_mark: Successfully appended phrase to '{id}'");
      }
      else
      {
        await Context.User.SendMessageAsync($"Unable to find ID '{id}'");
      }
    }

    [Command("disable"), Alias("d"), Summary("Enable a reaction")]
    public async Task EnableReact(ulong id, bool state)
    {
      if (ReactionResource.Instance.Enable(Context.Guild.Id, id, state))
      {
        await ReplyAsync(":white_check_mark: Successfully " + (state ? "enabled" : "disabled") + $" ID '{id}'");
      }
    }

    [Command("remove"), Alias("r"), Summary("Remove reaction")]
    public async Task RemoveReaction(ulong id)
    {
      if (ReactionResource.Instance.Pop(Context.Guild.Id, id))
      {
        await ReplyAsync($":white_check_mark: Successfully removed '{id}'");
      }
      else
      {
        await Context.User.SendMessageAsync($"Unable to find ID '{id}'");
      }
    }
  }
}
