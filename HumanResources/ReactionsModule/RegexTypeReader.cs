using Discord.Commands;
using HumanResources.Utilities;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HumanResources.ReactionsModule
{
  public class RegexTypeReader : TypeReader
  {
    public override Task<TypeReaderResult> ReadAsync(ICommandContext ctx, string input, IServiceProvider services)
    {
      try
      {
        var result = new Regex(input);
        if (result != null)
        {
          return Task.FromResult(TypeReaderResult.FromSuccess(result));
        }
      }
      catch (Exception e)
      {
        LogUtil.Write("RegexTypeReader:ReadAsync", e.Message);
      }
      return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as a regular expression"));
    }
  }
}
