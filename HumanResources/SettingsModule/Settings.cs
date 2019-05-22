﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HumanResources.AdminModule;
using HumanResources.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace HumanResources.Settings
{
  [Group("settings")]
  [RequireContext(ContextType.Guild)]
  [Remarks("Most of these functions are only accessible by guild administrators")]
  public class Settings : ModuleBase<SocketCommandContext>
  {
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

    [Command("prefix"), Summary("Set command prefix for the guild")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task SetPrefix([Summary("The new prefix")] char prefix)
    {
      Config.Bot.Guilds[Context.Guild.Id].Prefix = prefix;
      await ReplyAsync($":white_check_mark: Successfully set prefix to **{prefix}**");
    }

    [Command("mark"), Summary("Set mark for the guild")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task SetMark([Summary("The new mark")] char mark)
    {
      Config.Bot.Guilds[Context.Guild.Id].Mark = mark;
      await ReplyAsync($":white_check_mark: Successfully set mark to **{mark}**");
      await MarkResource.Instance.CheckSetGuild(Context.Guild);
    }

    [Command("marklist"), Summary("Set if marked members are also blacklisted or not")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task SetMarkList([Summary("True means all marked marked members are blacklisted while marked")] bool state)
    {
      Config.Bot.Guilds[Context.Guild.Id].MarkList = state;
      await ReplyAsync($":white_check_mark: Successfully set blacklist on mark to **{state}**");
    }

    [Group("welcome")]
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
        embed.AddField("Base role", $"**{Context.Guild.GetRole(Context.Guild.Roles.Where(x => x.Position == cfg.Position).Select(x => x.Id).Single()).Name}**", true);
        embed.AddField("Message", cfg.Message);
        embed.WithFooter(LogUtil.LogTime);

        await ReplyAsync("", false, embed.Build());
      }

      [Command("enable"), Summary("Enable or disable the welcome functionality")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task SetEnable([Summary("Set to true to enable")] bool state)
      {
        Config.Bot.Guilds[Context.Guild.Id].Welcome.Enabled = state;
        await ReplyAsync(":white_check_mark: Successfully " + (state ? "enabled" : "disabled") + " welcome functionality");
      }

      [Command("time"), Summary("Set the amount of minutes it takes before new users are given full server privileges")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task SetTime([Summary("The new time in minutes")] uint time)
      {
        Config.Bot.Guilds[Context.Guild.Id].Welcome.Time = time;
        await ReplyAsync($":white_check_mark: Successfully set welcome time to **{time} minutes**");
      }

      [Command("role"), Summary("Set the original role of new users")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task SetRole([Summary("The new original role")] IRole role)
      {
        Config.Bot.Guilds[Context.Guild.Id].Welcome.Position = role.Position;
        await ReplyAsync($":white_check_mark: Successfully set original role to **{role.Name}**");
      }

      [Command("message"), Summary("Set the welcome message")]
      public async Task SetMessage([Summary("The new welcome message, leave empty to disable functionality")] [Remainder] string msg = "")
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
    }
  }
}
