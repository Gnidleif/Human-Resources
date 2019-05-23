using Discord;
using Discord.Commands;
using HumanResources.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HumanResources.TwitterModule
{
  [Group("twitter")]
  public class Twitter : ModuleBase<SocketCommandContext>
  {
    [Command, Summary("Retrieves a Twitter user specified by handle/id")]
    public async Task GetUser(string identifier)
    {
      var user = await TwitterResource.Instance.GetUserAsync(identifier);
      if (user != null)
      {
        var embed = new EmbedBuilder();
        embed.WithAuthor($"{user.Name} (@{user.ScreenNameResponse})", user.ProfileImageUrl, $"https://twitter.com/{user.ScreenNameResponse}");
        embed.WithDescription(user.Description);
        if (uint.TryParse(user.ProfileLinkColor.Replace("#", ""), System.Globalization.NumberStyles.HexNumber, null, out uint rgb))
        {
          embed.WithColor(new Color(rgb));
        }
        embed.WithFooter($"ID: {user.UserIDResponse}", TwitterResource.Instance.Icon);

        await ReplyAsync("", false, embed.Build());
      }
    }

    [Group("stalk")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Stalk : ModuleBase<SocketCommandContext>
    {
      [Command, Summary("Start outputting any tweets a specified user does in the given channel")]
      public async Task StalkUser(string identifier, IMessageChannel ch)
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
        var any = false;
        var embed = new EmbedBuilder();
        foreach(var c in Context.Guild.Channels)
        {
          var l = await TwitterResource.Instance.GetUsersByChannelIdAsync(c.Id);
          if (l.Any())
          {
            embed.AddField($"#{c.Name}", string.Join(", ", l.Select(x => $"[{x.ScreenNameResponse}](https://www.twitter.com/{x.ScreenNameResponse})").ToList()));
            any = true;
          }
        }
        if (any)
        {
          embed.WithColor(56, 161, 243);
          embed.WithFooter(LogUtil.LogTime, TwitterResource.Instance.Icon);
          await ReplyAsync("", false, embed.Build());
        }
      }
    }
  }
}
