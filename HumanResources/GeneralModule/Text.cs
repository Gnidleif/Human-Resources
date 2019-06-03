using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HumanResources.TextModule
{
  public class Text : ModuleBase<SocketCommandContext>
  {
    [Command("spongebob"), Alias("sb"), Summary("Spongebobbify text")]
    public async Task Spongebob([Remainder] string text)
    {
      var result = text.Select(c => new Random(DateTime.UtcNow.Millisecond).Next(0, 2) == 0 
        ? c.ToString().ToLower() 
        : c.ToString().ToUpper())
        .ToList();
      var user = Context.User as SocketGuildUser;
      var embed = new EmbedBuilder();
      embed.WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
      embed.WithColor(247, 235, 98);
      embed.WithDescription(string.Join("", result));
      await ReplyAsync("", false, embed.Build());
    }

    [Command("8ball"), Alias("8b"), Summary("Ask questions, get answers")]
    public async Task EightBall([Remainder] string text)
    {
      if (!new Regex(@"(?mi).{3,}[?]$").IsMatch(text))
      {
        await Context.User.SendMessageAsync("Questions are at least three characters long and end with a question mark");
        return;
      }
      var answers = new string[]
      {
        "It is certain",
        "It is decidedly so",
        "Without a doubt",
        "Yes - definitely",
        "You may rely on it",
        "As I see it, yes",
        "Most likely",
        "Outlook good",
        "Yes",
        "Signs point to yes",
        "Reply hazy, try again",
        "Ask again later",
        "Better not tell you now",
        "Cannot predict now",
        "Concentrate and ask again",
        "Don't count on it",
        "My reply is no",
        "My sources say no",
        "Outlook not so good",
        "Very doubtful",
      };
      await ReplyAsync(answers[new Random(DateTime.UtcNow.Millisecond).Next(0, answers.Length)]);
    }
  }
}
