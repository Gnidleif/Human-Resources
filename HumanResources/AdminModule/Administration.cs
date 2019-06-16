﻿using Discord;
using Discord.Commands;
using HumanResources.Utilities;
using System;
using System.Threading.Tasks;

namespace HumanResources.AdminModule
{
  [RequireContext(ContextType.Guild)]
  public class Administration : ModuleBase<SocketCommandContext>
  {
    #region General
    [Command("kick"), Alias("k"), Summary("Kicks the specified user")]
    [RequireBotPermission(GuildPermission.KickMembers)]
    [RequireUserPermission(GuildPermission.KickMembers)]
    public async Task KickUser(IGuildUser user, [Remainder] string reason = "")
    {
      try
      {
        await user.KickAsync(reason);
      }
      catch (Discord.Net.HttpException e)
      {
        LogUtil.Write("Administration:KickUser", e.Message);
        await Context.User.SendMessageAsync(e.Message);
        return;
      }

      var embed = new EmbedBuilder();
      embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
      embed.WithDescription("User kicked");
      embed.AddField("Judge", Context.User.Username, true);
      if (!string.IsNullOrEmpty(reason))
      {
        embed.AddField("Reason", reason, true);
      }

      await ReplyAsync("", false, embed.Build());
    }

    [Command("ban"), Alias("b"), Summary("Bans the specified user")]
    [RequireBotPermission(GuildPermission.BanMembers)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    public async Task BanUser(IGuildUser user,[Remainder] string reason = "")
    {
      try
      {
        await user.BanAsync(0, reason);
      }
      catch (Discord.Net.HttpException e)
      {
        LogUtil.Write("Administration:BanUser", e.Message);
        await Context.User.SendMessageAsync(e.Message);
        return;
      }

      var embed = new EmbedBuilder();
      embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
      embed.WithDescription("User banned");
      embed.AddField("Judge", Context.User.Username, true);
      if (!string.IsNullOrEmpty(reason))
      {
        embed.AddField("Reason", reason, true);
      }

      await ReplyAsync("", false, embed.Build());
    }

    [Command("purge"), Alias("p"), Summary("Removes specified amount of messages in given channel")]
    [RequireBotPermission(GuildPermission.ManageMessages)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task PurgeChannel(uint count = 1)
    {
      var messages = await Context.Channel.GetMessagesAsync(Context.Message.Id, Direction.Before, (int)count).FlattenAsync();
      var ch = Context.Channel as ITextChannel;
      await Context.Message.DeleteAsync();
      await ch.DeleteMessagesAsync(messages);
      var m = await ReplyAsync($":white_check_mark: Successfully removed maximum {count} messages");
      await Task.Delay(5000);
      await m.DeleteAsync();
    }
    #endregion

    #region Voice
    [Group("voice"), Alias("v"), Summary("Commands related to voice chat")]
    public class Voice : ModuleBase<SocketCommandContext>
    {
      [Command("kick"), Alias("k"), Summary("Disconnect user from voice chat")]
      [RequireBotPermission(GuildPermission.ManageChannels | GuildPermission.MoveMembers)]
      [RequireUserPermission(GuildPermission.KickMembers)]
      public async Task KickUser(IGuildUser user,[Remainder] string reason = "")
      {
        if (user.VoiceChannel == null)
        {
          await Context.User.SendMessageAsync($"{user.Username} is not connected to a voice channel");
          return;
        }

        try
        {
          await user.ModifyAsync(x => x.Channel = null);
        }
        catch (Discord.Net.HttpException e)
        {
          LogUtil.Write("Voice:KickUser", e.Message);
          await Context.User.SendMessageAsync(e.Message);
          return;
        }

        var embed = new EmbedBuilder();
        embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
        embed.WithDescription("User voice kicked");
        embed.AddField("Judge", Context.User.Username, true);
        if (!string.IsNullOrEmpty(reason))
        {
          embed.AddField("Reason", reason, true);
        }

        await ReplyAsync("", false, embed.Build());
      }

      [Group("mute"), Alias("m"), Summary("Functions related to muting users")]
      [RequireBotPermission(GuildPermission.MuteMembers)]
      [RequireUserPermission(GuildPermission.MuteMembers)]
      public class Mute : ModuleBase<SocketCommandContext>
      {
        [Command, Summary("Mutes the specified user")]
        public async Task MuteUser(IGuildUser user,[Remainder] string reason = "")
        {
          if (user.IsMuted == true || user.VoiceChannel == null)
          {
            await Context.User.SendMessageAsync($"Unable to mute {user.Username}");
            return;
          }
          try
          {
            await user.ModifyAsync(x => x.Mute = true);
          }
          catch (Exception e)
          {
            LogUtil.Write("Voice:MuteUser", e.Message);
            await Context.User.SendMessageAsync(e.Message);
            return;
          }

          var embed = new EmbedBuilder();
          embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
          embed.WithDescription("User muted");
          embed.AddField("Judge", Context.User.Username, true);
          if (!string.IsNullOrEmpty(reason))
          {
            embed.AddField("Reason", reason, true);
          }

          await ReplyAsync("", false, embed.Build());
        }

        [Command("remove"), Alias("r"), Summary("Unmutes the specified user")]
        public async Task UnmuteUser(IGuildUser user)
        {
          if (user.IsSelfMuted == true || user.IsMuted == false || user.VoiceChannel == null)
          {
            await Context.User.SendMessageAsync($"Unable to unmute {user.Username}");
          }
          try
          {
            await user.ModifyAsync(x => x.Mute = false);
          }
          catch (Exception e)
          {
            LogUtil.Write("Voice:UnmuteUser", e.Message);
            await Context.User.SendMessageAsync(e.Message);
            return;
          }
        }
      }

      [Group("deafen"), Alias("d"), Summary("Functions related to deafening users")]
      [RequireBotPermission(GuildPermission.DeafenMembers)]
      [RequireUserPermission(GuildPermission.DeafenMembers)]
      public class Deafen : ModuleBase<SocketCommandContext>
      {
        [Command, Summary("Deafens the specified user")]
        public async Task DeafenUser(IGuildUser user,[Remainder] string reason = "")
        {
          if (user.IsDeafened == true || user.VoiceChannel == null)
          {
            await Context.User.SendMessageAsync($"Unable to deafen {user.Username}");
            return;
          }
          try
          {
            await user.ModifyAsync(x => x.Deaf = true);
          }
          catch (Exception e)
          {
            LogUtil.Write("Voice:DeafenUser", e.Message);
            await Context.User.SendMessageAsync(e.Message);
            return;
          }

          var embed = new EmbedBuilder();
          embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
          embed.WithDescription("User deafened");
          embed.AddField("Judge", Context.User.Username, true);
          if (!string.IsNullOrEmpty(reason))
          {
            embed.AddField("Reason", reason, true);
          }

          await ReplyAsync("", false, embed.Build());
        }

        [Command("remove"), Alias("r"), Summary("Undeafens the specified user")]
        public async Task UndeafenUser([Summary("The user to undeafen")] IGuildUser user)
        {
          if (user.IsSelfDeafened == true || user.IsDeafened == false || user.VoiceChannel == null)
          {
            await Context.User.SendMessageAsync($"Unable to undeafen {user.Username}");
          }
          try
          {
            await user.ModifyAsync(x => x.Deaf = false);
          }
          catch (Exception e)
          {
            LogUtil.Write("Voice:UndeafenUser", e.Message);
            await Context.User.SendMessageAsync(e.Message);
            return;
          }
        }
      }
    }
    #endregion

    #region Blacklist
    [Group("blacklist"), Alias("bl"), Summary("Handles the guild blacklist")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Blacklist : ModuleBase<SocketCommandContext>
    {
      [Command, Summary("Blacklists user from bot usage")]
      public async Task BlacklistUser(IGuildUser user,[Remainder] string reason = "")
      {
        if (BlacklistResource.Instance.Push(user.GuildId, user.Id))
        {
          var embed = new EmbedBuilder();
          embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
          embed.WithDescription("User blacklisted");
          embed.AddField("Judge", Context.User.Username, true);
          if (!string.IsNullOrEmpty(reason))
          {
            embed.AddField("Reason", reason, true);
          }

          await ReplyAsync("", false, embed.Build());
        }
      }

      [Command("remove"), Alias("r"), Summary("Remove user from blacklist")]
      public async Task WhitelistUser(IGuildUser user)
      {
        if (!BlacklistResource.Instance.Pop(user.GuildId, user.Id))
        {
          await Context.User.SendMessageAsync($"{user.Username} is already blacklisted");
        }
      }
    }
    #endregion

    #region Timeout
    [Group("timeout"), Alias("to"), Summary("Handles putting and removing people from timeout")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Timeout : ModuleBase<SocketCommandContext>
    {
      [Command, Summary("Sets the specified user in timeout")]
      [RequireBotPermission(GuildPermission.ManageRoles)]
      public async Task TimeoutUser(IGuildUser user,uint minutes = 10, [Remainder] string reason = "")
      {
        if (minutes == 0)
        {
          minutes = (uint)new Random((int)LogUtil.ToUnixTime()).Next(10, 5000);
        }
        try
        {
          await TimeoutResource.Instance.SetTimeout(user, minutes);
          if (user.VoiceChannel != null)
          {
            await user.ModifyAsync(x => x.Channel = null);
          }
        }
        catch(Exception e)
        {
          LogUtil.Write("Timeout:TimeoutUser", e.Message);
          return;
        }
        var embed = new EmbedBuilder();
        embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
        embed.WithDescription("User set on timeout");
        embed.AddField("Judge", Context.User.Username, true);
        embed.AddField("Minutes", minutes, true);
        if (!string.IsNullOrEmpty(reason))
        {
          embed.AddField("Reason", reason, true);
        }

        await ReplyAsync("", false, embed.Build());
      }

      [Command("remove"), Alias("r"), Summary("Removes timeout from specified user")]
      [RequireBotPermission(GuildPermission.ManageRoles)]
      public async Task UntimeoutUser(IGuildUser user)
      {
        try
        {
          await TimeoutResource.Instance.UnsetTimeout(user);
        }
        catch (Exception e)
        {
          LogUtil.Write("Timeout:UntimeoutUser", e.Message);
          await Context.User.SendMessageAsync($"Couldn't remove {user.Username} from timeout");
        }
      }

      [Command("setup"), Alias("s"), Summary("Sets up the guild for timeout usage")]
      [RequireBotPermission(GuildPermission.ManageRoles)]
      public async Task Setup()
      {
        var everyone = Context.Guild.EveryoneRole;
        await everyone.ModifyAsync(x => 
        {
          x.Mentionable = true;
          x.Permissions = new GuildPermissions(true, false, false, false, false, false, false, false, true, false, false, false, false, false, true, false, false, false, false, false, false, false, true, false, true, false, false, false, false);
        });
        await Context.User.SendMessageAsync("Successfully set up the @everyone role to use the timeout functionality");
      }
    }
    #endregion

    #region Mark
    [Group("mark"), Alias("mk"), Summary("Handles marking specific users on the guild")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Mark : ModuleBase<SocketCommandContext>
    {
      [Command, Summary("Marks specified user")]
      [RequireBotPermission(GuildPermission.ManageNicknames)]
      public async Task MarkUser(IGuildUser user, string reason = "")
      {
        if (Config.Bot.Guilds[user.GuildId].MarkList)
        {
          BlacklistResource.Instance.Push(user.GuildId, user.Id);
        }
        if (MarkResource.Instance.Push(user.GuildId, user.Id))
        {
          var mark = Config.Bot.Guilds[user.GuildId].Mark;
          await MarkResource.Instance.CheckSet(user, mark);

          var embed = new EmbedBuilder();
          embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
          embed.WithDescription("User marked");
          embed.AddField("Judge", Context.User.Username, true);
          if (!string.IsNullOrEmpty(reason))
          {
            embed.AddField("Reason", reason, true);
          }

          await ReplyAsync("", false, embed.Build());
        }
      }

      [Command("remove"), Alias("r"), Summary("Unmark specified user")]
      [RequireBotPermission(GuildPermission.ManageNicknames)]
      public async Task UnmarkUser(IGuildUser user)
      {
        if (Config.Bot.Guilds[user.GuildId].MarkList)
        {
          BlacklistResource.Instance.Pop(user.GuildId, user.Id);
        }
        if (MarkResource.Instance.Pop(user.GuildId, user.Id))
        {
          try
          {
            await user.ModifyAsync(x => x.Nickname = "");
          }
          catch (Discord.Net.HttpException e)
          {
            LogUtil.Write("Mark:UnmarkUser", e.Message);
          }
        }
      }
    }
    #endregion
  }
}
