using Discord;
using Discord.Commands;
using HumanResources.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HumanResources.TwitterModule
{
  [Group("twitter"), Alias("tw")]
  public class Twitter : ModuleBase<SocketCommandContext>
  {
    [Command, Summary("Retrieves a Twitter user specified by handle/id")]
    public async Task GetUser(string identifier, bool verbose = false)
    {
      var user = await TwitterResource.Instance.GetUserAsync(identifier);
      if (user != null)
      {
        var embed = new EmbedBuilder();
        embed.WithAuthor($"{user.Name} (@{user.ScreenNameResponse})", user.ProfileImageUrl, $"https://twitter.com/{user.ScreenNameResponse}");
        if (uint.TryParse(user.ProfileLinkColor.Replace("#", ""), System.Globalization.NumberStyles.HexNumber, null, out uint rgb))
        {
          embed.WithColor(new Color(rgb));
        }

        if (verbose == true)
        {
          embed.WithDescription(user.Description);

          embed.AddField("Created", LogUtil.FormattedDate(user.CreatedAt), true);
          embed.AddField("Age", user.FormattedAge(), true);
          embed.AddField("Location", !string.IsNullOrEmpty(user.Location) ? user.Location : "-", true);

          embed.AddField("Verified", user.Verified);

          embed.AddField("Followers", user.FollowersCount, true);
          embed.AddField("Following", user.FriendsCount, true);
          embed.AddField("Ratio", user.Ratio().ToString("0.00"), true);

          embed.AddField("Protected", user.Protected);

          embed.AddField("Tweets", user.StatusesCount, true);
          embed.AddField("Per day", user.TweetsPerDay().ToString("0.00"), true);
          embed.AddField("Last", user.Status != null ? $"[{LogUtil.FormattedDate(user.Status.CreatedAt)}](https://www.twitter.com/{user.ScreenNameResponse}/status/{user.Status.StatusID})" : "-", true);

          var defs = "-";
          if (user.DefaultProfile)
          {
            defs = "Using default theme color";
          }
          if (user.DefaultProfileImage)
          {
            defs += (defs.Length > 0 ? " and" : "Using deafault") + " profile image";
          }
          embed.AddField("Defaults", defs);

          embed.AddField("Favorites", user.FavoritesCount, true);
          embed.AddField("Per day", user.FavsPerDay().ToString("0.00"), true);
          embed.AddField("URL", !string.IsNullOrEmpty(user.Url) ? user.Url : "-", true);
        }

        embed.AddField("Score", (long)user.Score());
        embed.WithFooter($"ID: {user.UserIDResponse}", TwitterResource.Instance.Icon);

        await ReplyAsync("", false, embed.Build());
      }
    }

    // deactivated until fixed
    [Group("stalk")]
    [RequireUserPermission(GuildPermission.Administrator)]
    private class Stalk : ModuleBase<SocketCommandContext>
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
