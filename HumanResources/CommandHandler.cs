﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HumanResources.AdminModule;
using HumanResources.ReactionsModule;
using HumanResources.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
      char prefix = Config.Bot.Guilds[ctx.Guild.Id].Prefix;
      if (msg.HasMentionPrefix(Global.Client.CurrentUser, ref argPos))
      {
        var markov = Config.Bot.Guilds[ctx.Guild.Id].Markov;
        await MarkovTalk(ctx, (int)markov.Step, (int)markov.Count);
      }
      else if (msg.HasCharPrefix(prefix, ref argPos))
      {
        var result = await this.Service.ExecuteAsync(ctx, argPos, null);
        if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
        {
          LogUtil.Write("Client_MessageReceived", $"Message: {msg.Content} | Error: {result.ErrorReason}");
          await ctx.User.SendMessageAsync(result.ErrorReason);
        }
      }
    }

    private async Task MarkovTalk(SocketCommandContext ctx, int step, int wordCount)
    {
      var messages = await ctx.Channel.GetMessagesAsync(500).FlattenAsync();
      if (!messages.Any())
      {
        return;
      }
      var prefixPattern = $"^{Config.Bot.Guilds[ctx.Guild.Id].Prefix}\\w+";
      var pattern = @"(?m)(<(@[!&]?|[#]|a?:\w+:)\d+>)|(\bhttps://.+\b)";
      var filtered = new List<string>();
      foreach (var msg in messages.Where(x => !Regex.IsMatch(x.Content, prefixPattern) && !x.Author.IsBot))
      {
        var rep = Regex.Replace(msg.Content, pattern, "");
        if (string.IsNullOrEmpty(rep))
        {
          continue;
        }
        var split = Regex.Split(rep, @"\s+");
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
      if (!filtered.Any())
      {
        return;
      }
      var chain = new Dictionary<string, List<string>>();
      for (var i = 0; i < filtered.Count - step; i += step)
      {
        var k = filtered[i].ToLower();
        var v = string.Join(" ", filtered.Skip(i + 1).Take(step)).ToLower();
        if (!chain.ContainsKey(k))
        {
          chain.Add(k, new List<string> { v });
        }
        else
        {
          chain[k].Add(v);
        }
      }
      var rand = new Random(DateTime.UtcNow.Millisecond);
      var result = new List<string>();
      var key = chain.ElementAt(rand.Next(0, chain.Count)).Key;
      while (result.Count < (wordCount / step))
      {
        var value = chain[key][rand.Next(0, chain[key].Count)];
        key = chain.ContainsKey(value) ? value : chain.ElementAt(rand.Next(0, chain.Count)).Key;
        result.Add(value);
      }
      await ctx.Channel.SendMessageAsync(string.Join(" ", result));
    }
  }
}
