using System;
using System.Globalization;

namespace HumanResources.Utilities
{
  class LogUtil
  {
    public static string LogTime => DateTime.Now.ToString(Config.Bot.TimeFormat, CultureInfo.InvariantCulture);
    public static long ToUnixTime() => new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
    public static void Write(string source, string message) => Console.WriteLine($"[{LogTime} at {source}]: {message}");

    public static Tuple<int, int, int, int, int, int> CalculateAge(DateTime origin)
    {
      var now = DateTime.Now;
      int years = new DateTime(now.Subtract(origin).Ticks).Year - 1;
      var pyd = origin.AddYears(years);
      int months = 0;
      for (int i = 0; i <= 12; i++)
      {
        if (pyd.AddMonths(i) == now)
        {
          months = i;
          break;
        }
        else if (pyd.AddMonths(i) >= now)
        {
          months = i - 1;
          break;
        }
      }
      int days = now.Subtract(pyd.AddMonths(months)).Days;
      int hours = now.Subtract(pyd).Hours;
      int minutes = now.Subtract(pyd).Minutes;
      int seconds = now.Subtract(pyd).Seconds;

      return new Tuple<int, int, int, int, int, int>(years, months, days, hours, minutes, seconds);
    }

    public static string FormattedDate(DateTime origin)
    {
      return origin.ToString(Config.Bot.TimeFormat, CultureInfo.InvariantCulture);
    }
  }
}
