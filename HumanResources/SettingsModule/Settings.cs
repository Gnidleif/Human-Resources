using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HumanResources.MarkModule;
using HumanResources.Utilities;
using System.Threading.Tasks;

namespace HumanResources.Settings
{
    [Remarks("Most of these functions are only accessible by guild administrators")]
    public class Settings : ModuleBase<SocketCommandContext>
    {
        [Command("getsettings"), Summary("Returns the bot settings for the guild")]
        public async Task GetSettings()
        {
            var guildCfg = Config.Bot.Guilds[Context.Guild.Id];
            var user = Context.User as SocketGuildUser;

            var embed = new EmbedBuilder();
            embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl());
            embed.AddField("Prefix", $"**{guildCfg.Prefix}**", true);
            embed.AddField("Mark", $"**{guildCfg.Mark}**", true);
            embed.WithFooter(LogUtil.Time);

            await ReplyAsync("", false, embed.Build());
        }

        [Command("setprefix"), Summary("Set command prefix for the guild")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task SetPrefix(char prefix)
        {
            var oldCfg = Config.Bot.Guilds[Context.Guild.Id];
            if (oldCfg.Prefix == prefix)
            {
                await ReplyAsync($":negative_squared_cross_mark: Prefix is already set to {prefix}");
                return;
            }
            var newCfg = new GuildConfig
            {
                Mark = oldCfg.Mark,
                Prefix = prefix,
            };
            if (Config.Update(Context.Guild.Id, newCfg))
            {
                await ReplyAsync($":white_check_mark: Successfully set prefix to **{prefix}**");
            }
            else
            {
                await ReplyAsync($":negative_squared_cross_mark: Failed setting prefix to **{prefix}**");
            }
        }

        [Command("setmark"), Summary("Set mark for the guild")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task SetMark(char mark)
        {
            var oldCfg = Config.Bot.Guilds[Context.Guild.Id];
            if (oldCfg.Mark == mark)
            {
                await ReplyAsync($":negative_squared_cross_mark: Prefix is already set to {mark}");
                return;
            }
            var newCfg = new GuildConfig
            {
                Mark = mark,
                Prefix = oldCfg.Prefix,
            };
            if (Config.Update(Context.Guild.Id, newCfg))
            {
                MarkResource.Instance.MarkAll();
                await ReplyAsync($":white_check_mark: Successfully set mark to **{mark}**");
            }
            else
            {
                await ReplyAsync($":negative_squared_cross_mark: Failed setting mark to **{mark}**");
            }
        }
    }
}
