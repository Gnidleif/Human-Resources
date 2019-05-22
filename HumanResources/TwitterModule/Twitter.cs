using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace HumanResources.TwitterModule
{
  [Group("twitter")]
  public class Twitter : ModuleBase<SocketCommandContext>
  {
    [Command, Summary("Returns info on a twitter user specified by handle")]
    public async Task GetUser([Summary("The relevant handle")] string handle)
    {
      await Task.CompletedTask;
    }

    [Command, Summary("Returns info on a twitter user specified by ID")]
    public async Task GetUser([Summary("The relevant ID")] ulong id)
    {
      await Task.CompletedTask;
    }

    [Group("stalk")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Stalk : ModuleBase<SocketCommandContext>
    {
      [Command, Summary("Stalk a twitter user specified by handle, output in given channel")]
      public async Task StalkUser([Summary("The relevant handle")] string user, IMessageChannel channel)
      {
        await Task.CompletedTask;
      }

      [Command, Summary("Stalk a twitter user specified by ID, output in given channel")]
      public async Task StalkUser([Summary("The relevant ID")] ulong user, IMessageChannel channel)
      {
        if (TwitterResource.Instance.Push(Context.Guild.Id, channel.Id, user))
        {
          await ReplyAsync(":white_check_mark: User added");
        }

        await Task.CompletedTask;
      }

      [Command("remove"), Summary("Remove output from twitter user given a handle")]
      public async Task UnstalkUser([Summary("The relevant handle")] string user, IMessageChannel channel)
      {
        await Task.CompletedTask;
      }

      [Command("remove"), Summary("Remove output from twitter user given a handle")]
      public async Task UnstalkUser([Summary("The relevant ID")] ulong id, IMessageChannel channel)
      {
        await Task.CompletedTask;
      }

      [Command("list"), Summary("Returns guild list of stalked twitter users")]
      public async Task GetList()
      {
        await Task.CompletedTask;
      }
    }
  }
}
