using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HumanResources.AnnounceModule
{
  [Group("announce"), Alias("an")]
  [RequireContext(ContextType.Guild)]
  [RequireUserPermission(GuildPermission.Administrator)]
  public class Announce : ModuleBase<SocketCommandContext>
  {
    [Command("channel"), Alias("c"), Summary("Edit the output channel for guild announcements")]
    public async Task EditChannel(IChannel channel)
    {
      if (AnnounceResource.Instance.Push(Context.Guild.Id, channel.Id))
      {
        await ReplyAsync($":white_check_mark: Successfully added guild announcement feature");
      }
      else if (AnnounceResource.Instance.SetChannel(Context.Guild.Id, channel.Id))
      {
        await ReplyAsync($":white_check_mark: Successfully changed guild announcement channel to <#{channel.Id}>");
      }
      else
      {
        await Context.User.SendMessageAsync($":negative_squared_cross_mark: Editing guild announcement channel failed");
      }
    }

    [Command("enable"), Alias("e"), Summary("Edit the enabled/disabled state of a certain guild announcement")]
    public async Task EditState(string name, bool state)
    {
      if (AnnounceResource.Instance.SetState(Context.Guild.Id, name, state))
      {
        await ReplyAsync(":white_check_mark: Successfully " + (state == true ? "enabled" : "disabled") + $" the {name} announcement");
      }
      else
      {
        await Context.User.SendMessageAsync($":negative_squared_cross_mark: Unable to set state of the {name} announcement");
      }
    }

    [Command("message"), Alias("m"), Summary("Edit the announcement message of a certain guild announcement")]
    public async Task EditMsg(string name, [Remainder] string msg)
    {
      if (AnnounceResource.Instance.SetMsg(Context.Guild.Id, name, msg))
      {
        await ReplyAsync($":white_check_mark: Successfully edited the {name} announcement message");
      }
      else
      {
        await Context.User.SendMessageAsync($":negative_squared_cross_mark: Unable to edit the {name} announcement message");
      }
    }
  }
}
