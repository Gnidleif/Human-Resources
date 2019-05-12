using Discord.WebSocket;

namespace HumanResources
{
    internal static class Global
    {
        internal static string ResourceFolder { get; } = "Resources";
        internal static DiscordSocketClient Client { get; set; }
    }
}
