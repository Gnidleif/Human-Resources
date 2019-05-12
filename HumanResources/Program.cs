using Discord.WebSocket;
using HumanResources.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using HumanResources.MarkModule;
using System.Collections.Generic;
using HumanResources.AdminModule;

namespace HumanResources
{
    class Program
    {
        private CommandHandler Handler { get; set; }
        private List<IStaticResource> Resources { get; set; }

        private static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            if (string.IsNullOrEmpty(Config.Bot.Token))
            {
                Console.WriteLine("No token detected, please provice a valid token: ");
                var input = Console.ReadLine();
                Config.Bot = new BotConfig
                {
                    Token = input,
                    TimeFormat = Config.Bot.TimeFormat,
                    Guilds = Config.Bot.Guilds,
                };
            }

            Global.Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = Discord.LogSeverity.Verbose,
            });

            Global.Client.Log += Client_Log;
            Global.Client.LeftGuild += Client_LeftGuild;
            Global.Client.JoinedGuild += Client_JoinedGuild;
            Global.Client.Ready += Client_Ready;
            Global.Client.Disconnected += Client_Disconnected;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            try
            {
                await Global.Client.LoginAsync(Discord.TokenType.Bot, Config.Bot.Token);
                await Global.Client.StartAsync();
            }
            catch (Discord.Net.HttpException e)
            {
                Config.Bot = new BotConfig
                {
                    Token = "",
                    TimeFormat = Config.Bot.TimeFormat,
                    Guilds = Config.Bot.Guilds,
                };
                LogUtil.Write("MainAsync", e.Message);
                return;
            }

            this.Resources = new List<IStaticResource>
            {
                MarkResource.Instance,
                BlacklistResource.Instance,
            };

            this.Handler = new CommandHandler();
            await this.Handler.InitializeAsync();

            await Task.Delay(-1);
        }

        private async Task Client_Log(Discord.LogMessage arg)
        {
            LogUtil.Write(arg.Source, arg.Message);
            await Task.CompletedTask;
        }

        private async Task Client_LeftGuild(SocketGuild arg)
        {
            if (Config.DeleteGuild(arg.Id))
            {
                LogUtil.Write("Client_LeftGuild", $"Successfully removed {arg.Id} from Config");
            }

            if (MarkResource.Instance.PopGuild(arg.Id))
            {
                LogUtil.Write("Client_LeftGuild", $"Successfully removed {arg.Id} from MarkHandler");
            }

            await Task.CompletedTask;
        }

        private async Task Client_JoinedGuild(SocketGuild arg)
        {
            if (Config.AddGuild(arg.Id))
            {
                LogUtil.Write("Client_JoinedGuild", $"Successfully joined {arg.Id}");
            }

            await Task.CompletedTask;
        }

        private async Task Client_Ready()
        {
            foreach(var id in Global.Client.Guilds.Select(x => x.Id))
            {
                _ = Config.AddGuild(id);
            }

            _ = Config.Save();
            foreach(var r in this.Resources)
            {
                _ = r.Save();
            }

            _ = MarkResource.Instance.Start();
            MarkResource.Instance.MarkAll();

            await Task.CompletedTask;
        }

        private async Task Client_Disconnected(Exception arg)
        {
            _ = Config.Save();
            foreach(var r in this.Resources)
            {
                _ = r.Save();
            }

            await Task.CompletedTask;
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            _ = Config.Save();
            foreach(var r in this.Resources)
            {
                _ = r.Close();
            }
        }
    }
}
