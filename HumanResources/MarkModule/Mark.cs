using Discord;
using Discord.Commands;
using HumanResources.Utilities;
using System.Threading.Tasks;

namespace HumanResources.MarkModule
{
    public class Mark : ModuleBase<SocketCommandContext>
    {
        [Command("mark"), Summary("Marks specified user")]
        [RequireBotPermission(GuildPermission.ChangeNickname)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task MarkUser(IGuildUser user)
        {
            if (MarkResource.Instance.Push(user.GuildId, user.Id))
            {
                var mark = Config.Bot.Guilds[user.GuildId].Mark;
                await MarkResource.Instance.CheckSet(user, mark);
            }
        }

        [Command("unmark"), Summary("Unmark specified user")]
        [RequireBotPermission(GuildPermission.ChangeNickname)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task UnmarkUser(IGuildUser user)
        {
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
}