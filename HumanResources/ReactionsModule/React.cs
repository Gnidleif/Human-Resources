using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HumanResources.Utilities;
using System;
using System.Collections.Generic;
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
    public async Task GetReactions()
    {
      var l = ReactionResource.Instance.GetGuild(Context.Guild.Id);
      var sb = new StringBuilder();
      foreach(var rh in l)
      {
        sb.AppendLine($"{rh.Rgx.ToString()}: {rh.Phrase}");
      }
      var embed = new EmbedBuilder();
      var user = Context.User as SocketGuildUser;
      embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
      embed.WithDescription(sb.ToString());
      embed.WithFooter(LogUtil.LogTime);
      await ReplyAsync("", false, embed.Build());
    }

    [Command("add"), Alias("a"), Summary("Add a reaction to the guild")]
    public async Task AddReaction(Regex rgx, [Remainder] string phrase)
    {
      if (ReactionResource.Instance.Push(Context.Guild.Id, rgx, phrase))
      {
        await ReplyAsync($":white_check_mark: Successfully added '{rgx}'");
      }
      else
      {
        await Context.User.SendMessageAsync($"Unable to add duplicate of '{rgx}'");
      }
    }

    [Command("remove"), Alias("r"), Summary("Remove reaction")]
    public async Task RemoveReaction(string key)
    {
      ulong hash = (ulong)key.GetHashCode();
      if (ReactionResource.Instance.Pop(Context.Guild.Id, hash))
      {
        await ReplyAsync($":white_check_mark: Successfully removed '{key}'");
      }
    }
  }
}
