using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HumanResources.TextModule
{
  public class Text : ModuleBase<SocketCommandContext>
  {
    [Command("spongebob"), Alias("sb"), Summary("Spongebobbify text")]
    public async Task Spongebob([Remainder] string text)
    {
      var rand = new Random(DateTime.UtcNow.Millisecond);
      var result = text.Select(c => rand.Next(0, 2) == 0 ? c.ToString().ToLower() : c.ToString().ToUpper()).ToList();
      var user = Context.User as SocketGuildUser;
      var embed = new EmbedBuilder();
      embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
      embed.WithColor(247, 235, 98);
      embed.WithDescription(string.Join("", result));
      await ReplyAsync("", false, embed.Build());
    }
  }
}
