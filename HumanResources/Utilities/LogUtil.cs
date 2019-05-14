using System;
using System.Globalization;

namespace HumanResources.Utilities
{
  class LogUtil
  {
    public static string LogTime => DateTime.Now.ToString(Config.Bot.TimeFormat, CultureInfo.InvariantCulture);
    public static long ToUnixTime() => new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
    public static void Write(string source, string message) => Console.WriteLine($"[{LogTime} at {source}]: {message}");
  }
}
