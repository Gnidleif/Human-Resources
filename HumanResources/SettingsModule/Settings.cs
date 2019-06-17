using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HumanResources.AdminModule;
using HumanResources.AnnounceModule;
using HumanResources.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace HumanResources.Settings
{
  [Group("settings"), Alias("s")]
  [RequireContext(ContextType.Guild)]
  public class Settings : ModuleBase<SocketCommandContext>
  {
    #region Basic
    [Command, Summary("Returns the bot settings for the guild")]
    public async Task GetSettings()
    {
      var cfg = Config.Bot.Guilds[Context.Guild.Id];
      var user = Context.User as SocketGuildUser;

      var embed = new EmbedBuilder();
      embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl());
      embed.AddField("Prefix", $"**{cfg.Prefix}**", true);
      embed.AddField("Mark", $"**{cfg.Mark}**", true);
      embed.AddField("Blacklist on mark", $"**{cfg.MarkList}**", true);
      embed.WithFooter(LogUtil.LogTime);

      await ReplyAsync("", false, embed.Build());
    }

    [Command("prefix"), Alias("px"), Summary("Set command prefix for the guild")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task SetPrefix(char prefix)
    {
      Config.Bot.Guilds[Context.Guild.Id].Prefix = prefix;
      await ReplyAsync($":white_check_mark: Successfully set prefix to **{prefix}**");
    }

    [Command("mark"), Alias("mk"), Summary("Set mark for the guild")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task SetMark(char mark)
    {
      Config.Bot.Guilds[Context.Guild.Id].Mark = mark;
      await ReplyAsync($":white_check_mark: Successfully set mark to **{mark}**");
      await MarkResource.Instance.CheckSetGuild(Context.Guild);
    }

    [Command("marklist"), Alias("ml"), Summary("Set if marked members are also blacklisted or not")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task SetMarkList(bool state)
    {
      Config.Bot.Guilds[Context.Guild.Id].MarkList = state;
      var gid = Context.Guild.Id;
      MarkResource.Instance.GetUsersByGuild(gid).ForEach(uid => 
      {
        _ = state == true ? BlacklistResource.Instance.Push(gid, uid) : BlacklistResource.Instance.Pop(gid, uid);
      });
      await ReplyAsync($":white_check_mark: Successfully set blacklist on mark to **{state}**");
    }

    [Command("reset"), Alias("re"), Summary("Reset settings to defaults")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task ResetSettings()
    {
      var gc = new GuildConfig();
      Config.Bot.Guilds[Context.Guild.Id].Prefix = gc.Prefix;
      Config.Bot.Guilds[Context.Guild.Id].Mark = gc.Mark;
      Config.Bot.Guilds[Context.Guild.Id].MarkList = gc.MarkList;
      await ReplyAsync(":white_check_mark: Successfully reset prefix, mark and marklist settings");
    }
    #endregion

    #region Welcome
    [Group("welcome"), Alias("w")]
    public class Welcome : ModuleBase<SocketCommandContext>
    {
      [Command, Summary("Retrieve all welcome settings")]
      public async Task GetWelcome()
      {
        var cfg = Config.Bot.Guilds[Context.Guild.Id].Welcome;
        var user = Context.User as SocketGuildUser;
        var embed = new EmbedBuilder();
        embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl());
        embed.AddField("Enabled", $"**{cfg.Enabled}**", true);
        embed.AddField("Time", $"**{cfg.Time}**", true);
        embed.AddField("Base role", $"**{Context.Guild.GetRole(Context.Guild.Roles.Where(x => x.Position == cfg.Rank).Select(x => x.Id).Single()).Name}**", true);
        embed.AddField("Message", cfg.Message);
        embed.WithFooter(LogUtil.LogTime);

        await ReplyAsync("", false, embed.Build());
      }

      [Command("enable"), Alias("e"), Summary("Enable or disable the welcome functionality")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task SetEnable(bool state)
      {
        Config.Bot.Guilds[Context.Guild.Id].Welcome.Enabled = state;
        await ReplyAsync(":white_check_mark: Successfully " + (state ? "enabled" : "disabled") + " welcome functionality");
      }

      [Command("time"), Alias("t"), Summary("Set the amount of minutes it takes before new users are given full server privileges")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task SetTime(uint time)
      {
        Config.Bot.Guilds[Context.Guild.Id].Welcome.Time = time;
        await ReplyAsync($":white_check_mark: Successfully set welcome time to **{time} minutes**");
      }

      [Command("role"), Alias("r"), Summary("Set the first role of new users")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task SetRole(IRole role)
      {
        if (role.IsManaged || role == Context.Guild.EveryoneRole)
        {
          await ReplyAsync($":negative_squared_cross_mark: **{role.Name}** is an invalid starting role");
        }
        else
        {
          Config.Bot.Guilds[Context.Guild.Id].Welcome.Rank = role.Position;
          await ReplyAsync($":white_check_mark: Successfully set first role to **{role.Name}**");
        }
      }

      [Command("message"), Alias("m"), Summary("Set the welcome message")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task SetMessage([Remainder] string msg = "")
      {
        Config.Bot.Guilds[Context.Guild.Id].Welcome.Message = msg;
        if (!string.IsNullOrEmpty(msg))
        {
          await ReplyAsync($":white_check_mark: Successfully set new message to: '{msg}'");
        }
        else
        {
          await ReplyAsync($":white_check_mark: Successfully disabled welcome message");
        }
      }

      [Command("reset"), Alias("re"), Summary("Reset server welcome settings")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task Reset()
      {
        Config.Bot.Guilds[Context.Guild.Id].Welcome = new WelcomeConfig();
        await ReplyAsync(":white_check_mark: Successfully reset guild welcome settings");
      }
    }
    #endregion

    #region Markov
    [Group("markov"), Alias("m")]
    public class Markov : ModuleBase<SocketCommandContext>
    {
      [Command, Summary("Retrieve markov settings for guild")]
      public async Task GetMarkov()
      {
        var cfg = Config.Bot.Guilds[Context.Guild.Id].Markov;
        var user = Context.User as SocketGuildUser;
        var embed = new EmbedBuilder();
        embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl());
        embed.AddField("Step", $"**{cfg.Step}**", true);
        embed.AddField("Count", $"**{cfg.Count}**", true);
        embed.AddField("Source", $"**{cfg.Source}**", true);
        embed.AddField("Chance", $"**{cfg.Chance}%**", true);
        embed.WithFooter(LogUtil.LogTime);

        await ReplyAsync("", false, embed.Build());
      }

      [Command("step"), Alias("s"), Summary("Set markov step count")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task SetStep(uint step)
      {
        if (step < 1 || step > 15)
        {
          await ReplyAsync($":negative_squared_cross_mark: Allowed markov step range: 1-15");
        }
        else
        {
          Config.Bot.Guilds[Context.Guild.Id].Markov.Step = step;
          await ReplyAsync($":white_check_mark: Successfully set markov step to: {step}");
        }
      }

      [Command("count"), Alias("c"), Summary("Set markov word count")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task SetCount(uint count)
      {
        if (count < 5 || count > 50)
        {
          await ReplyAsync($":negative_squared_cross_mark: Allowed markov count range: 5-50");
        }
        else
        {
          Config.Bot.Guilds[Context.Guild.Id].Markov.Count = count;
          await ReplyAsync($":white_check_mark: Successfully set markov count to: {count}");
        }
      }

      [Command("source"), Alias("so"), Summary("Set markov source count")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task SetSource(uint source)
      {
        if (source < 50 || source > 5000)
        {
          await ReplyAsync($":negative_squared_cross_mark: Allowed markov source range: 50-5000");
        }
        else
        {
          Config.Bot.Guilds[Context.Guild.Id].Markov.Source = source;
          await ReplyAsync($":white_check_mark: Successfully set markov source count to: {source}");
        }
      }

      [Command("chance"), Alias("ch"), Summary("Set markov trigger chance")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task SetChance(uint chance)
      {
        if (chance > 100)
        {
          chance = 100;
        }
        Config.Bot.Guilds[Context.Guild.Id].Markov.Chance = chance;
        await ReplyAsync($":white_check_mark: Successfully set markov trigger chance to: {chance}%");
      }

      [Command("reset"), Alias("re"), Summary("Reset server markov settings")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task Reset()
      {
        Config.Bot.Guilds[Context.Guild.Id].Markov = new MarkovConfig();
        await ReplyAsync(":white_check_mark: Successfully reset guild markov settings");
      }
    }
    #endregion

    #region Announcement
    [Group("announcement"), Alias("an")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Announcement : ModuleBase<SocketCommandContext>
    {
      [Command, Summary("Get the guild announcement settings")]
      public async Task GetSettings()
      {
        var list = AnnounceResource.Instance.GetAnnouncements(Context.Guild.Id);
        if (list == null)
        {
          await Context.User.SendMessageAsync(":negative_squared_cross_mark: No announcement settings configured for guild, run <prefix>settings announcement channel <channel id> to get started");
          return;
        }

        var embed = new EmbedBuilder();
        var user = Context.User as SocketGuildUser;
        embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl());
        foreach(var r in list)
        {
          if (r.Value != null)
          {
            embed.AddField(r.Key, r.Value, true);
          }
          else
          {
            embed.WithDescription($"**Channel**: <#{r.Key}>");
          }
        }
        await ReplyAsync("", false, embed.Build());
      }

      [Command("channel"), Alias("ch"), Summary("Edit the output channel for guild announcements")]
      public async Task EditChannel(IChannel channel)
      {
        if (AnnounceResource.Instance.Push(Context.Guild.Id, channel.Id))
        {
          await ReplyAsync($":white_check_mark: Successfully added guild announcement feature");
        }
        else if (AnnounceResource.Instance.SetChannel(Context.Guild.Id, channel.Id))
        {
          await ReplyAsync($":white_check_mark: Successfully changed guild announcement channel to <#{channel.Id}>");
        }
        else
        {
          await Context.User.SendMessageAsync($":negative_squared_cross_mark: Editing guild announcement channel failed");
        }
      }

      [Command("enable"), Alias("e"), Summary("Edit the enabled/disabled state of a certain guild announcement")]
      public async Task EditState(string name, bool state)
      {
        if (AnnounceResource.Instance.SetState(Context.Guild.Id, name, state))
        {
          await ReplyAsync(":white_check_mark: Successfully " + (state == true ? "enabled" : "disabled") + $" the **{name}** announcement");
        }
        else
        {
          await Context.User.SendMessageAsync($":negative_squared_cross_mark: Unable to set state of the **{name}** announcement");
        }
      }

      [Command("message"), Alias("msg"), Summary("Edit the announcement message of a certain guild announcement")]
      public async Task EditMsg(string name, [Remainder] string msg)
      {
        if (AnnounceResource.Instance.SetMsg(Context.Guild.Id, name, msg))
        {
          await ReplyAsync($":white_check_mark: Successfully edited the **{name}** announcement message");
        }
        else
        {
          await Context.User.SendMessageAsync($":negative_squared_cross_mark: Unable to edit the **{name}** announcement message");
        }
      }
    }
    #endregion
  }
}
