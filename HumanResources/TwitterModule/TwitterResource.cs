using HumanResources.Utilities;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HumanResources.TwitterModule
{
  public class TwitterResource : IStaticResource
  {
    private static readonly Lazy<TwitterResource> lazy = new Lazy<TwitterResource>(() => new TwitterResource());
    private readonly string Path = $"{Global.ResourceFolder}/twitter.json";
    private TwitterInfo Info { get; set; } = new TwitterInfo();
    private TwitterContext Ctx { get; set; }

    public static TwitterResource Instance { get { return lazy.Value; } }
    public readonly string Icon = "https://images-ext-1.discordapp.net/external/bXJWV2Y_F3XSra_kEqIYXAAsI3m1meckfLhYuWzxIfI/https/abs.twimg.com/icons/apple-touch-icon-192x192.png";

    private TwitterResource()
    {
    }

    public bool Close()
    {
      var toDelete = this.Info.UserChannels.Keys.Where(x => !this.Info.UserChannels[x].Any()).ToList();
      toDelete.ForEach(x => this.Pop(x));

      return this.Save();
    }

    public bool Contains(ulong uid, ulong cid) => this.Info.UserChannels.ContainsKey(uid) && this.Info.UserChannels[uid].Contains(cid);

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
      }
    }

    private async Task Authenticate()
    {
      var auth = new SingleUserAuthorizer
      {
        CredentialStore = new SingleUserInMemoryCredentialStore
        {
          ConsumerKey = this.Info.ConsumerKey,
          ConsumerSecret = this.Info.ConsumerSecret,
          AccessToken = this.Info.AccessToken,
          AccessTokenSecret = this.Info.AccessTokenSecret,
        },
      };

      try
      {
        await auth.AuthorizeAsync();
        this.Ctx = new TwitterContext(auth);
      }
      catch (Exception e)
      {
        LogUtil.Write("TwitterResource:Authenticate", e.Message);
      }
    }

    public bool Pop(ulong uid, ulong cid)
    {
      if (this.Contains(uid, cid))
      {
        return this.Info.UserChannels[uid].Remove(cid);
      }
      return false;
    }

    public bool Pop(ulong uid) => this.Info.UserChannels.Remove(uid);

    public bool Push(ulong uid, ulong cid)
    {
      if (!this.Info.UserChannels.ContainsKey(uid))
      {
        this.Info.UserChannels.Add(uid, new HashSet<ulong>());
      }
      if (!this.Contains(uid, cid))
      {
        this.Info.UserChannels[uid].Add(cid);
        return true;
      }
      return false;
    }

    public bool Save() => JsonUtil.TryWrite(this.Path, this.Info);

    public async Task<User> GetUserAsync(string identifier)
    {
      User user = null;
      var query = this.Ctx.User.Where(t => t.Type == UserType.Show);
      try
      {
        if (ulong.TryParse(identifier, out ulong id))
        {
          user = await query.Where(t => t.UserID == id).SingleOrDefaultAsync();
        }
        return user ?? await query.Where(t => t.ScreenName == identifier).SingleOrDefaultAsync();
      }
      catch (Exception e)
      {
        LogUtil.Write("TwitterResources:GetUserAsync", e.Message);
      }
      return null;
    }

    public async Task<List<User>> GetUsersByChannelIdAsync(ulong cid)
    {
      var list = new List<User>();
      var ids = this.Info.UserChannels.Where(x => x.Value.Contains(cid)).Select(x => x.Key).ToList();
      if (ids.Any())
      {
        list = await this.Ctx.User
          .Where(t => t.Type == UserType.Lookup)
          .Where(t => t.UserIdList == string.Join(",", ids))
          .ToListAsync();
      }
      return list;
    }
  }

  public class TwitterInfo
  {
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string AccessTokenSecret { get; set; } = string.Empty;
    public Dictionary<ulong, HashSet<ulong>> UserChannels { get; set; } = new Dictionary<ulong, HashSet<ulong>>();
  }
}
