using HumanResources.Utilities;
using LinqToTwitter;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HumanResources.TwitterModule
{
  public class TwitterResource : IStaticResource
  {
    private static readonly Lazy<TwitterResource> lazy = new Lazy<TwitterResource>(() => new TwitterResource());
    private const string Name = "twitter.json";
    private readonly string Path = $"{Global.ResourceFolder}/{Name}";
    private TwitterConfig Cfg { get; set; } = new TwitterConfig();
    private TwitterContext Ctx { get; set; }

    public static TwitterResource Instance { get { return lazy.Value; } }

    private TwitterResource()
    {
    }

    public bool Close()
    {
      throw new NotImplementedException();
    }

    public bool Contains(ulong gid, ulong uid)
    {
      throw new NotImplementedException();
    }

    public async Task Initialize()
    {
      if (!Directory.Exists(Global.ResourceFolder))
      {
        Directory.CreateDirectory(Global.ResourceFolder);
      }
      var temp = new TwitterConfig();
      if (File.Exists(this.Path) ? JsonUtil.TryRead(this.Path, out temp) : JsonUtil.TryWrite(this.Path, temp))
      {
        this.Cfg = temp;
        await this.Authenticate();
      }
    }

    private async Task Authenticate()
    {
      var auth = new SingleUserAuthorizer
      {
        CredentialStore = new SingleUserInMemoryCredentialStore
        {
          ConsumerKey = this.Cfg.ConsumerKey,
          ConsumerSecret = this.Cfg.ConsumerSecret,
          AccessToken = this.Cfg.AccessToken,
          AccessTokenSecret = this.Cfg.AccessTokenSecret,
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

    public bool Pop(ulong gid, ulong uid)
    {
      throw new NotImplementedException();
    }

    public bool Pop(ulong gid)
    {
      throw new NotImplementedException();
    }

    public bool Push(ulong gid, ulong uid)
    {
      throw new NotImplementedException();
    }

    public bool Save() => JsonUtil.TryWrite(this.Path, this.Cfg);
  }

  public class TwitterConfig
  {
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string AccessTokenSecret { get; set; } = string.Empty;
  }
}
