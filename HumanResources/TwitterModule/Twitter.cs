using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace HumanResources.TwitterModule
{
  [Group("twitter")]
  public class Twitter : ModuleBase<SocketCommandContext>
  {
    [Command, Summary("Retrieves a Twitter user specified by handle/id")]
    public async Task GetUser([Remainder] string identifier)
    {
      var user = await TwitterResource.Instance.GetUserAsync(identifier);
      if (user != null)
      {
        var embed = new EmbedBuilder();
        embed.WithAuthor($"{user.Name} (@{user.ScreenNameResponse})", user.ProfileImageUrl, $"https://twitter.com/{user.ScreenNameResponse}");
        var rgb = uint.Parse(user.ProfileLinkColor.Replace("#", ""), System.Globalization.NumberStyles.HexNumber);
        embed.WithDescription(user.Description);
        embed.WithColor(new Color(rgb));
        embed.WithFooter("ID: " + user.UserIDResponse, "https://images-ext-1.discordapp.net/external/bXJWV2Y_F3XSra_kEqIYXAAsI3m1meckfLhYuWzxIfI/https/abs.twimg.com/icons/apple-touch-icon-192x192.png");

        await ReplyAsync("", false, embed.Build());
      }
    }

    [Group("stalk")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Stalk : ModuleBase<SocketCommandContext>
    {
      [Command, Summary("Start outputting any tweets a specified user does in the given channel")]
      public async Task StalkUser(IMessageChannel ch, [Remainder] string identifier)
      {
        var user = await TwitterResource.Instance.GetUserAsync(identifier);
        if (user != null)
        {
          var id = ulong.Parse(user.UserIDResponse);
          if (TwitterResource.Instance.Push(id, ch.Id))
          {
            await ReplyAsync($":white_check_mark: Started stalking {user.Name} (@{user.ScreenNameResponse})");
          }
        }
      }

      [Command("list"), Summary("Return list of stalked users")]
      public async Task StalkList()
      {
        var embed = new EmbedBuilder();
        foreach(var c in Context.Guild.Channels)
        {
          var l = TwitterResource.Instance.GetUsersByChannelId(c.Id);
          if (l.Any())
          {
            embed.AddField(c.Name, string.Join(", ", l.Select(x => x.ScreenNameResponse)));
          }
        }
        await ReplyAsync("", false, embed.Build());
      }
    }
  }
}
