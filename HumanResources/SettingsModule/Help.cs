using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace HumanResources.SettingsModule
{
  public class Help : ModuleBase<SocketCommandContext>
  {
    [Command("help"), Alias("h"), Summary("Returns link to Github repo")]
    public async Task GetHelp()
    {
      var embed = new EmbedBuilder();
      embed.WithAuthor("Github project page", Context.Client.CurrentUser.GetAvatarUrl() ?? Context.Client.CurrentUser.GetDefaultAvatarUrl(), "https://github.com/Gnidleif/Human-Resources");
      await ReplyAsync("", false, embed.Build());
    }
  }
}
