using Discord;
using Discord.Commands;
using HumanResources.Utilities;
using System;
using System.Threading.Tasks;

namespace HumanResources.AdminModule
{
  public class Administration : ModuleBase<SocketCommandContext>
  {
    [Command("kick"), Alias("k"), Summary("Kicks specified user")]
    [RequireBotPermission(GuildPermission.KickMembers)]
    [RequireUserPermission(GuildPermission.KickMembers)]
    public async Task KickUser(IGuildUser user, [Remainder] string reason = "None")
    {
      try
      {
        await user.KickAsync(reason);
      }
      catch (Discord.Net.HttpException e)
      {
        LogUtil.Write("Administration:KickUser", e.Message);
      }

      var embed = new EmbedBuilder();
      embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
      embed.WithDescription("User kicked");
      embed.AddField("Judge", Context.User.Username, true);
      embed.AddField("Reason", reason, true);

      await ReplyAsync("", false, embed.Build());
    }

    [Command("ban"), Alias("b"), Summary("Bans specified user")]
    [RequireBotPermission(GuildPermission.BanMembers)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    public async Task BanUser(IGuildUser user, [Remainder] string reason = "None")
    {
      try
      {
        await user.BanAsync(0, reason);
      }
      catch (Discord.Net.HttpException e)
      {
        LogUtil.Write("Administration:BanUser", e.Message);
      }

      var embed = new EmbedBuilder();
      embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
      embed.WithDescription("User banned");
      embed.AddField("Judge", Context.User.Username, true);
      embed.AddField("Reason", reason, true);

      await ReplyAsync("", false, embed.Build());
    }

    [Command("voicekick"), Alias("vk"), Summary("Disconnect user from voice chat")]
    [RequireBotPermission(GuildPermission.ManageChannels | GuildPermission.MoveMembers)]
    [RequireUserPermission(GuildPermission.KickMembers)]
    public async Task VoiceKickUser(IGuildUser user, [Remainder] string reason = "None")
    {
      if (user.VoiceChannel == null)
      {
        await Context.User.SendMessageAsync($"{user.Username} is not connected to a voice channel");
        return;
      }

      try
      {
        var temp = await Context.Guild.CreateVoiceChannelAsync(LogUtil.ToUnixTime().ToString());
        await user.ModifyAsync(x => x.ChannelId = temp.Id);
        await temp.DeleteAsync();
      }
      catch (Discord.Net.HttpException e)
      {
        LogUtil.Write("Administration:VoiceKickUser", e.Message);
      }

      var embed = new EmbedBuilder();
      embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
      embed.WithDescription("User voice kicked");
      embed.AddField("Judge", Context.User.Username, true);
      embed.AddField("Reason", reason, true);

      await ReplyAsync("", false, embed.Build());
    }

    [Command("blacklist"), Alias("bl"), Summary("Blacklists user from bot usage")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task BlacklistUser(IGuildUser user, [Remainder] string reason = "None")
    {
      if (BlacklistResource.Instance.Push(user.GuildId, user.Id))
      {
        var embed = new EmbedBuilder();
        embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
        embed.WithDescription("User blacklisted");
        embed.AddField("Judge", Context.User.Username, true);
        embed.AddField("Reason", reason, true);

        await ReplyAsync("", false, embed.Build());
      }
    }

    [Command("whitelist"), Alias("wl"), Summary("Restore bot access to user")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task WhitelistUser(IGuildUser user)
    {
      if (BlacklistResource.Instance.Pop(user.GuildId, user.Id))
      {
        await ReplyAsync($"{Context.User.Mention} restored bot access to {user.Mention}");
      }
    }

    [Command("timeout"), Summary("Sets the specified user in timeout")]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.Administrator)]
    public async Task TimeoutUser(IGuildUser user, uint minutes = 10)
    {
      if (minutes == 0)
      {
        minutes = (uint)new Random((int)LogUtil.ToUnixTime()).Next(10, 5000);
      }
      if (TimeoutResource.Instance.SetTimeout(user, minutes))
      {
        var embed = new EmbedBuilder();
        embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
        embed.WithDescription("User set on timeout");
        embed.AddField("Judge", Context.User.Username, true);
        embed.AddField("Minutes", minutes, true);

        await ReplyAsync("", false, embed.Build());
      }
    }

    [Command("untimeout"), Summary("Removes timeout from specified user")]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task UntimeoutUser(IGuildUser user)
    {
      if (!TimeoutResource.Instance.UnsetTimeout(user))
      {
        await Context.User.SendMessageAsync($"Couldn't remove {user.Username} from timeout");
      }
    }
  }
}
