using Discord;
using Discord.Commands;
using HumanResources.Utilities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResources.TwitterModule
{
  [Group("twitter"), Alias("tw")]
  public class Twitter : ModuleBase<SocketCommandContext>
  {
    [Command("search"), Alias("s"), Summary("Retrieves a Twitter user specified by handle/id")]
    public async Task GetUser(string identifier, bool verbose = false)
    {
      var user = TwitterResource.Instance.GetUser(identifier);
      if (user != null)
      {
        var embed = new EmbedBuilder();
        embed.WithAuthor($"{user.Name} (@{user.ScreenName})", user.ProfileImageUrl, $"https://twitter.com/{user.ScreenName}");
        if (uint.TryParse(user.ProfileLinkColor.Replace("#", ""), NumberStyles.HexNumber, null, out uint rgb))
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
          embed.AddField("Ratio", user.Ratio().ToString("0.00", CultureInfo.InvariantCulture), true);

          embed.AddField("Protected", user.Protected);

          embed.AddField("Tweets", user.StatusesCount, true);
          embed.AddField("Per day", user.TweetsPerDay().ToString("0.00", CultureInfo.InvariantCulture), true);
          embed.AddField("Last", user.Status != null ? $"[{LogUtil.FormattedDate(user.Status.CreatedAt)}](https://www.twitter.com/{user.ScreenName}/status/{user.Status.IdStr})" : "-", true);

          var defs = "-";
          if (user.DefaultProfile)
          {
            defs = "Using default theme color";
          }
          if (user.DefaultProfileImage)
          {
            defs += (defs.Length > 1 ? " and" : "Using default") + " profile image";
          }
          embed.AddField("Defaults", defs);

          embed.AddField("Favorites", user.FavouritesCount, true);
          embed.AddField("Per day", user.FavsPerDay().ToString("0.00", CultureInfo.InvariantCulture), true);
          embed.AddField("URL", !string.IsNullOrEmpty(user.Url) ? user.Url : "-", true);
        }

        embed.AddField("Score", (long)user.Score());
        embed.WithFooter($"ID: {user.IdStr}", TwitterResource.Instance.Icon);

        await ReplyAsync("", false, embed.Build());
      }
    }

    [Group("follow"), Alias("f"), Summary("Functions related to following people on twitter")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Follow : ModuleBase<SocketCommandContext>
    {
      [Command, Summary("Follow a specific Twitter account, outputting their tweets in the specified channel")]
      public async Task FollowUser(string identifier, IChannel channel)
      {
        var user = TwitterResource.Instance.GetUser(identifier);
        if (user == null)
        {
          await Context.User.SendMessageAsync($":negative_squared_cross_mark: Couldn't find Twitter user with identifier {identifier}");
          return;
        }

        if (TwitterResource.Instance.Push((ulong)user.Id, channel.Id))
        {
          await ReplyAsync($":white_check_mark: Successfully following {user.ScreenName} in <#{channel.Id}>");
        }
        else
        {
          await Context.User.SendMessageAsync($":negative_squared_cross_mark: Already following {user.ScreenName} in {channel}");
        }
      }

      [Command("remove"), Alias("r"), Summary("Stop following a user in a specific channel")]
      public async Task UnfollowUser(string identifier, IChannel channel)
      {
        var user = TwitterResource.Instance.GetUser(identifier);
        if (user == null)
        {
          await Context.User.SendMessageAsync($":negative_squared_cross_mark: Couldn't find Twitter user with identifier {identifier}");
          return;
        }

        if (TwitterResource.Instance.Pop((ulong)user.Id, channel.Id))
        {
          await ReplyAsync($":white_check_mark: Successfully unfollowed {user.ScreenName} in <#{channel.Id}>");
        }
        else
        {
          await Context.User.SendMessageAsync($":negative_squared_cross_mark: Already not following {user.ScreenName} in {channel}");
        }
      }

      [Command("list"), Alias("l"), Summary("List all followed user in the guild")]
      public async Task ListUsers()
      {
        var guild = Context.Guild as IGuild;
        var chans = (await guild.GetTextChannelsAsync()).Select(x => x.Id).ToList();
        var list = TwitterResource.Instance.GetStreamList(chans);
        if (list.Any())
        {
          var gu = Context.User as IGuildUser;
          var embed = new EmbedBuilder();
          embed.WithAuthor(gu.Nickname ?? gu.Username, gu.GetAvatarUrl() ?? gu.GetDefaultAvatarUrl());
          var sb = new StringBuilder();
          foreach(var item in list)
          {
            var formatted = new List<string>();
            foreach(var user in item.Value)
            {
              formatted.Add($"[{user.ScreenName}]({user.Url})");
            }
            sb.AppendLine($"<#{item.Key}>: {string.Join(", ", formatted)}");
          }
          embed.WithDescription(sb.ToString());
          await ReplyAsync("", false, embed.Build());
        }
      }
    }
  }
}
