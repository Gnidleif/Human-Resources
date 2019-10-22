using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HumanResources.Utilities;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HumanResources.ReactionsModule
{
  [RequireContext(ContextType.Guild)]
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

      var user = Context.User as SocketGuildUser;
      var desc = $"```json\n{json}\n```";
      try
      {
        if (id == default)
        {
          await user.SendMessageAsync(desc);
        }
        else
        {
          var embed = new EmbedBuilder();
          embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
          embed.WithDescription(desc);
          embed.WithFooter(LogUtil.LogTime);
          await ReplyAsync("", false, embed.Build());
        }
      }
      catch (Exception e)
      {
        await Context.User.SendMessageAsync(e.Message);
      }
      finally
      {
        await Context.Message.DeleteAsync();
      }
    }

    [RequireUserPermission(GuildPermission.Administrator)]
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

    [RequireUserPermission(GuildPermission.Administrator)]
    [Command("remove"), Alias("r"), Summary("Remove reaction")]
    public async Task RemoveReaction(ulong id)
    {
      if (ReactionResource.Instance.Pop(Context.Guild.Id, id))
      {
        await ReplyAsync($":white_check_mark: Successfully removed '{id}'");
      }
      else
      {
        await Context.User.SendMessageAsync($"Unable to find '{id}'");
      }
    }

    [Group("modify"), Alias("m"), Summary("Modify reactions")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class ModifyReact : ModuleBase<SocketCommandContext>
    {
      [Command("add"), Alias("a"), Summary("Add a phrase to already existing ID")]
      public async Task AddPhrase(ulong id, [Remainder] string phrase)
      {
        if (ReactionResource.Instance.Append(Context.Guild.Id, id, phrase))
        {
          await ReplyAsync($":white_check_mark: Successfully appended phrase to '{id}'");
        }
        else
        {
          await Context.User.SendMessageAsync($"Unable to find '{id}'");
        }
      }

      [Command("remove"), Alias("r"), Summary("Remove phrase at specified ID and index (zero-indexed)")]
      public async Task RemovePhrase(ulong id, uint idx)
      {
        if (ReactionResource.Instance.Pop(Context.Guild.Id, id, (int)idx))
        {
          await ReplyAsync($":white_check_mark: Successfully removed phrase #{idx}");
        }
        else
        {
          await Context.User.SendMessageAsync($"Unable to remove phrase");
        }
      }

      [Command("phrase"), Alias("p"), Summary("Modify phrase at specified ID and index (zero-indexed)")]
      public async Task ModifyPhrase(ulong id, uint idx, [Remainder] string phrase)
      {
        if (ReactionResource.Instance.Modify(Context.Guild.Id, id, (int)idx, phrase))
        {
          await ReplyAsync($":white_check_mark: Successfully modified phrase #{idx}");
        }
        else
        {
          await Context.User.SendMessageAsync($"Modification failed");
        }
      }

      [Command("regex"), Alias("re"), Summary("Modify regex at specified ID")]
      public async Task ModifyRegex(ulong id, Regex rgx)
      {
        if (ReactionResource.Instance.Modify(Context.Guild.Id, id, rgx))
        {
          await ReplyAsync($":white_check_mark: Successfully modified regex on '{id}'");
        }
        else
        {
          await Context.User.SendMessageAsync($"Modification failed");
        }
      }

      [Command("enable"), Alias("e"), Summary("Enable a reaction")]
      public async Task EnableReact(ulong id, bool state)
      {
        if (ReactionResource.Instance.Modify(Context.Guild.Id, id, state))
        {
          await ReplyAsync(":white_check_mark: Successfully " + (state ? "enabled" : "disabled") + $" ID '{id}'");
        }
      }
    }
  }
}
