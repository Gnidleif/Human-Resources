using System;
using System.Globalization;

namespace HumanResources.Utilities
{
    class LogUtil
    {
        public static string Time
        {
            get
            {
                return DateTime.Now.ToString(Config.Bot.TimeFormat, CultureInfo.InvariantCulture);
            }
        }

        public static ulong UnixTime()
        {
            return (ulong)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 1, 0, 0, 0)).TotalMilliseconds);
        }

        public static ulong UnixTime(DateTime from)
        {
            return (ulong)(from.Subtract(new DateTime(1970, 1, 1, 1, 0, 0, 0)).TotalMilliseconds);
        }

        public static void Write(string source, string message)
        {
            Console.WriteLine($"[{Time} at {source}]: {message}");
        }
    }
}
