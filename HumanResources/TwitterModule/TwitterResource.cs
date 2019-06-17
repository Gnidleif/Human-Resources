using HumanResources.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace HumanResources.TwitterModule
{
  public class TwitterResource : IStaticResource
  {
    private static readonly Lazy<TwitterResource> lazy = new Lazy<TwitterResource>(() => new TwitterResource());
    private readonly string Path = $"{Global.ResourceFolder}/twitter.json";
    private TwitterInfo Info { get; set; } = new TwitterInfo();
    private ITwitterCredentials Credentials { get; set; }
    private Tweetinvi.Streaming.IFilteredStream Stream { get; set; }
    private Thread StreamThread { get; set; }

    public static TwitterResource Instance { get { return lazy.Value; } }
    public readonly string Icon = "https://images-ext-1.discordapp.net/external/bXJWV2Y_F3XSra_kEqIYXAAsI3m1meckfLhYuWzxIfI/https/abs.twimg.com/icons/apple-touch-icon-192x192.png";

    private TwitterResource()
    {
    }

    public bool Close()
    {
      var toDelete = this.Info.List.Keys.Where(x => !this.Info.List[x].Any()).ToList();
      toDelete.ForEach(x => this.Pop(x));

      return this.Save();
    }

    public bool Contains(ulong uid, ulong cid) => this.Info.List.ContainsKey(uid) && this.Info.List[uid].Contains(cid);

    public async Task Initialize()
    {
      if (!Directory.Exists(Global.ResourceFolder))
      {
        Directory.CreateDirectory(Global.ResourceFolder);
      }
      var temp = new TwitterInfo();
      if (File.Exists(this.Path) ? JsonUtil.TryRead(this.Path, out temp) : JsonUtil.TryWrite(this.Path, temp))
      {
        this.Info = temp;
        await this.Authenticate();

        this.Stream.StreamPaused += (sender, args) =>
        {
          LogUtil.Write("TwitterResource:StreamPaused", "Stream conditions not met");
        };

        this.Stream.StreamStopped += (sender, args) =>
        {
          if (args.Exception != null)
          {
            LogUtil.Write("TwitterResource:StreamStopped", args.Exception.Message);
          }
          if (args.DisconnectMessage != null)
          {
            LogUtil.Write("TwitterResource:StreamStopped", args.DisconnectMessage.Reason);
          }
        };

        this.Stream.StreamStarted += (sender, args) =>
        {
          LogUtil.Write("TwitterResource:StreamStarted", "Stream conditions met");
        };

        this.Stream.MatchingTweetReceived += async (sender, args) =>
        {
          if (!args.Tweet.IsRetweet && 
            (args.MatchOn == Tweetinvi.Streaming.MatchOn.Follower || args.MatchOn == (Tweetinvi.Streaming.MatchOn.Follower | Tweetinvi.Streaming.MatchOn.FollowerInReplyTo)))
          {
            var user = User.GetUserFromId(args.Tweet.CreatedBy.Id);
            var embed = new Discord.EmbedBuilder();
            embed.WithAuthor($"{user.Name} (@{user.ScreenName})", user.ProfileImageUrl, args.Tweet.Url);
            embed.WithDescription(args.Tweet.FullText);
            embed.WithColor(new Discord.Color(56, 161, 243));
            embed.WithFooter($"{LogUtil.FormattedDate(args.Tweet.CreatedAt)}", this.Icon);
            var build = embed.Build();

            foreach(var cid in this.Info.List[(ulong)user.Id])
            {
              var ch = Global.Client.GetChannel(cid) as Discord.IMessageChannel;
              if (ch != null)
              {
                await ch.SendMessageAsync("", false, build);
              }
            }
          }
        };

        foreach (var uid in this.Info.List.Keys)
        {
          this.StartFollowing(uid);
        }
      }
    }

    private async Task Authenticate()
    {
      this.Credentials = new TwitterCredentials(this.Info.ConsumerKey, this.Info.ConsumerSecret, this.Info.AccessToken, this.Info.AccessTokenSecret);
      try
      {
        Auth.SetCredentials(this.Credentials);
        this.Stream = Tweetinvi.Stream.CreateFilteredStream(this.Credentials);
      }
      catch (Exception e)
      {
        LogUtil.Write("TwitterResource:Authenticate", e.Message);
      }
      await Task.CompletedTask;
    }

    public bool Pop(ulong uid, ulong cid)
    {
      if (this.Contains(uid, cid))
      {
        this.Info.List[uid].Remove(cid);
        if (!this.Info.List[uid].Any())
        {
          this.StopFollowing(uid);
        }
        return true;
      }
      return false;
    }

    public bool Pop(ulong uid) => this.Info.List.Remove(uid);

    public bool Push(ulong uid, ulong cid)
    {
      if (!this.Info.List.ContainsKey(uid))
      {
        this.Info.List.Add(uid, new HashSet<ulong>());
      }
      if (!this.Contains(uid, cid))
      {
        this.Info.List[uid].Add(cid);
        _ = this.StartFollowing(uid);
        return true;
      }
      return false;
    }

    public bool Save() => JsonUtil.TryWrite(this.Path, this.Info);

    public IUser GetUser(string identifier)
    {
      IUser u = null;
      if (long.TryParse(identifier, out long id))
      {
        u = User.GetUserFromId(id);
      }
      return u ?? User.GetUserFromScreenName(identifier);
    }

    private bool StartFollowing(ulong uid)
    {
      var id = (long)uid;
      if (!this.Stream.ContainsFollow(id))
      {
        this.Stream.AddFollow(id);
        this.SafeStartStream();
        return true;
      }
      return false;
    }

    private bool StopFollowing(ulong uid)
    {
      var id = (long)uid;
      if (this.Stream.ContainsFollow(id))
      {
        this.Stream.RemoveFollow(id);
        if (!this.Stream.FollowingUserIds.Any())
        {
          this.SafeStopStream();
        }
        return true;
      }
      return false;
    }

    private void SafeStartStream()
    {
      if (this.Stream.StreamState == StreamState.Running || !this.Stream.FollowingUserIds.Any())
      {
        return;
      }
      this.StreamThread = new Thread(new ThreadStart(() =>
      {
        this.Stream.StartStreamMatchingAllConditions();
      }));
      this.StreamThread.Start();
    }

    private bool SafeStopStream()
    {
      if (this.Stream.StreamState == StreamState.Running)
      {
        this.Stream.StopStream();
        return true;
      }
      return false;
    }
  }

  public class TwitterInfo
  {
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string AccessTokenSecret { get; set; } = string.Empty;
    public Dictionary<ulong, HashSet<ulong>> List { get; set; } = new Dictionary<ulong, HashSet<ulong>>();
  }
}
