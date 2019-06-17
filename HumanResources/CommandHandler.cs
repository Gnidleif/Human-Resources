using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HumanResources.AdminModule;
using HumanResources.ReactionsModule;
using HumanResources.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HumanResources
{
  class CommandHandler
  {
    private CommandService Service { get; set; }

    public async Task InitializeAsync()
    {
      this.Service = new CommandService(new CommandServiceConfig
      {
        CaseSensitiveCommands = false,
        DefaultRunMode = RunMode.Async,
        LogLevel = Discord.LogSeverity.Verbose,
      });

      this.Service.AddTypeReader(typeof(Regex), new RegexTypeReader());

      await this.Service.AddModulesAsync(Assembly.GetEntryAssembly(), null);

      Global.Client.MessageReceived += Client_MessageReceived;
    }

    private async Task Client_MessageReceived(SocketMessage arg)
    {
      var msg = arg as SocketUserMessage;
      if (msg == null)
      {
        return;
      }

      var ctx = new SocketCommandContext(Global.Client, msg);
      if (ctx.User.IsBot)
      {
        return;
      }

      var resp = string.Join("\n", ReactionResource.Instance.Find(ctx.Guild.Id, msg.Content));
      if (!string.IsNullOrEmpty(resp))
      {
        await ctx.Channel.SendMessageAsync(resp);
      }

      if (BlacklistResource.Instance.Contains(ctx.Guild.Id, ctx.User.Id))
      {
        return;
      }

      int argPos = 0;
      var settings = Config.Bot.Guilds[ctx.Guild.Id];
      if (msg.HasCharPrefix(settings.Prefix, ref argPos) || msg.HasMentionPrefix(Global.Client.CurrentUser, ref argPos))
      {
        var result = await this.Service.ExecuteAsync(ctx, argPos, null);
        if (!result.IsSuccess)
        {
          LogUtil.Write("Client_MessageReceived", $"Message: {msg.Content} | Error: {result.ErrorReason}");
          await ctx.User.SendMessageAsync(result.ErrorReason);
        }
      }
      else if (new Random(DateTime.UtcNow.Millisecond).Next(1, 100 + 1) <= settings.Markov.Chance)
      {
        using (ctx.Channel.EnterTypingState())
        {
          await MarkovTalk(ctx, (int)settings.Markov.Source, (int)settings.Markov.Step, (int)settings.Markov.Count);
        }
      }
    }

    private async Task MarkovTalk(SocketCommandContext ctx, int source, int step, int wordCount)
    {
      var messages = await ctx.Channel.GetMessagesAsync(source).FlattenAsync();
      if (!messages.Any())
      {
        return;
      }
      var filtered = FilterMessages(ctx, messages);
      if (!filtered.Any())
      {
        return;
      }
      var chain = MakeChain(filtered, step);
      if (!chain.Any())
      {
        return;
      }
      var result = GenerateMessage(chain, step, wordCount);
      if (string.IsNullOrEmpty(result))
      {
        return;
      }
      await ctx.Channel.SendMessageAsync(result);
    }

    private List<string> FilterMessages(SocketCommandContext ctx, IEnumerable<IMessage> messages)
    {
      var control = @"[!?.,:;()[]]+";
      var prefix = $"^{Config.Bot.Guilds[ctx.Guild.Id].Prefix}\\w+";
      var filter = @"(?m)(<(@[!&]?|[#]|a?:\w+:)\d+>)|(\bhttps://.+\b)";
      var filtered = new List<string>();
      foreach (var msg in messages.Where(x => !Regex.IsMatch(x.Content, prefix) && !x.Author.IsBot))
      {
        var rep = Regex.Replace(msg.Content, filter, "");
        if (string.IsNullOrEmpty(rep))
        {
          continue;
        }
        var sb = new StringBuilder();
        foreach (var s in rep.Select(x => x.ToString()))
        {
          sb.Append(Regex.IsMatch(s, control) ? $" {s} " : s);
        }
        var split = Regex.Split(sb.ToString(), @"\s+");
        if (!split.Any())
        {
          continue;
        }
        var noEmpty = split.Where(x => !string.IsNullOrEmpty(x));
        if (!noEmpty.Any())
        {
          continue;
        }
        filtered.AddRange(noEmpty);
      }
      return filtered;
    }

    private Dictionary<string, List<string>> MakeChain(List<string> filtered, int step)
    {
      var chain = new Dictionary<string, List<string>>();
      for (var i = 0; i < filtered.Count - step; i++)
      {
        var k = string.Join(" ", filtered.Skip(i).Take(step)).ToLower();
        var v = filtered[i + step].ToLower();
        if (!chain.ContainsKey(k))
        {
          chain.Add(k, new List<string> { v });
        }
        else
        {
          chain[k].Add(v);
        }
      }
      return chain;
    }

    private string GenerateMessage(Dictionary<string, List<string>> chain, int step, int wordCount)
    {
      var control = @"[!?.,:;()[]]+";
      var rand = new Random(DateTime.UtcNow.Millisecond);
      var result = new StringBuilder();
      var temp = new List<string>
      {
        chain.ElementAt(rand.Next(0, chain.Count)).Key,
      };
      for (int i = 0; i < wordCount; i++)
      {
        var key = string.Join(" ", temp.Skip(i).Take(step));
        if (!chain.ContainsKey(key))
        {
          key = chain.ElementAt(rand.Next(0, chain.Count)).Key;
        }
        var value = chain[key].ElementAt(rand.Next(0, chain[key].Count));
        while (result.Length == 0 && Regex.IsMatch(value, control))
        {
          key = chain.ElementAt(rand.Next(0, chain.Count)).Key;
          value = chain[key].ElementAt(rand.Next(0, chain[key].Count));
        }
        temp.Add(value);
        result.Append(Regex.IsMatch(value, control) ? value : $" {value}");
      }
      return result.ToString();
    }
  }
}
