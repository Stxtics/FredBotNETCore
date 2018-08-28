using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace FredBotNETCore
{
    public class Program
    {

        #region Fields

        private DiscordSocketClient _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            MessageCacheSize = 100,
            LogLevel = LogSeverity.Verbose
        });
        private CommandHandler _commands = new CommandHandler();
        #endregion

        #region Startup

        public static IServiceProvider _provider;

        // Convert sync main to an async main.
        public static void Main(string[] args)
        {
            new Program().Start().GetAwaiter().GetResult();
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false }));

            var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);

            return provider;
        }

        public async Task Start()
        {
            string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "TextFiles");
            await _client.LoginAsync(tokenType: TokenType.Bot, token: new StreamReader(path: Path.Combine(downloadPath, "Token.txt")).ReadLine());
            await _client.StartAsync();

            _client.Log += Log;

            await _commands.Install(_client);

            var serviceProvider = ConfigureServices();
            _provider = serviceProvider;

            Task.WaitAny(Task.Factory.StartNew(() => CheckStatus()), Task.Factory.StartNew(() => GameLoop()));
            await Task.Delay(-1);
        }

        #endregion

        #region Timer Loop

        public static async Task CheckStatus()
        {
            HttpClient web = new HttpClient();
            string hint = Modules.Public.PublicModule.GetBetween(await web.GetStringAsync("http://pr2hub.com/files/artifact_hint.txt"), "{\"hint\":\"", "\",\"finder_name\":\"");
            string finder = Modules.Public.PublicModule.GetBetween(await web.GetStringAsync("http://pr2hub.com/files/artifact_hint.txt"), "\",\"finder_name\":\"", "\",\"updated_time\":");
            string time = Modules.Public.PublicModule.GetBetween(await web.GetStringAsync("http://pr2hub.com/files/artifact_hint.txt"), "\",\"updated_time\":", "}");
            bool valid = false;
            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    #region HH
                    string status = await web.GetStringAsync("http://pr2hub.com/files/server_status_2.txt");
                    string[] servers = status.Split('}');
                    string happyHour = "", guildId = "";

                    foreach (string server_name in servers)
                    {
                        guildId = Modules.Public.PublicModule.GetBetween(server_name, "guild_id\":\"", "\"");
                        if(guildId.Equals("0"))
                        {
                            happyHour = Modules.Public.PublicModule.GetBetween(server_name, "hour\":\"", "\"");
                            string serverName = Modules.Public.PublicModule.GetBetween(server_name, "server_name\":\"", "\"");
                            if (!serverName.Equals("Tournament"))
                            {
                                if (happyHour.Equals("1"))
                                {
                                    await CommandHandler.CheckStatusAsync(true, serverName);
                                }
                                else
                                {
                                    await CommandHandler.CheckStatusAsync(false, serverName);
                                }
                            }
                        }
                    }

                    #endregion

                    #region Arti
                    string artifactHint = await web.GetStringAsync("http://pr2hub.com/files/artifact_hint.txt");
                    if (!hint.Equals(Modules.Public.PublicModule.GetBetween(artifactHint, "{\"hint\":\"", "\",\"finder_name\":\"")))
                    {
                        hint = Modules.Public.PublicModule.GetBetween(artifactHint, "{\"hint\":\"", "\",\"finder_name\":\"");
                        if (!time.Equals(Modules.Public.PublicModule.GetBetween(artifactHint, "\",\"updated_time\":", "}")))
                        {
                            time = Modules.Public.PublicModule.GetBetween(artifactHint, "\",\"updated_time\":", "}");
                            valid = true;
                        }
                        if (valid)
                        {
                            await CommandHandler.AnnouceHintUpdatedAsync(hint, true);
                            valid = false;
                        }
                        else
                        {
                            await CommandHandler.AnnouceHintUpdatedAsync(hint, false);
                        }
                    }
                    if (!finder.Equals(Modules.Public.PublicModule.GetBetween(artifactHint, "\",\"finder_name\":\"", "\",\"updated_time\":")))
                    {
                        finder = Modules.Public.PublicModule.GetBetween(artifactHint, "\",\"finder_name\":\"", "\",\"updated_time\":");
                        if (finder.Length > 0)
                        {
                            await CommandHandler.AnnounceArtifactFoundAsync(finder);
                        }
                    }
                    #endregion

                    #region Invites
                    await CommandHandler.RemovePermInvitesAsync();
                    #endregion
                }
                catch (Exception)
                {
                    //ignore
                }
            }
        }

        public async Task GameLoop()
        {
            while (true)
            {
                await Task.Delay(new Random().Next(300000, 600000));
                var process = Process.GetCurrentProcess();
                var time = DateTime.Now - process.StartTime;
                var sb = new StringBuilder();
                if (time.Days > 0)
                {
                    sb.Append($"{time.Days}d ");  /*Pulls the Uptime in Days*/
                }
                if (time.Hours > 0)
                {
                    sb.Append($"{time.Hours}h ");  /*Pulls the Uptime in Hours*/
                }
                if (time.Minutes > 0)
                {
                    sb.Append($"{time.Minutes}m ");  /*Pulls the Uptime in Minutes*/
                }
                sb.Append($"{time.Seconds}s ");  /*Pulls the Uptime in Seconds*/
                await _client.SetGameAsync($"/help for {sb.ToString()}", null, type: ActivityType.Playing);
                await Task.Delay(new Random().Next(300000, 600000));
                await _client.SetGameAsync($"/help in {_client.Guilds.Count} servers", null, type: ActivityType.Watching);
                await Task.Delay(new Random().Next(300000, 600000));
                int users = 0;
                foreach (SocketGuild guild in _client.Guilds)
                {
                    users = users + guild.MemberCount;
                }
                await _client.SetGameAsync($"/help with {users} users", null, type: ActivityType.Listening);
            }
        }

        #endregion

        #region Log

        private Task Log(LogMessage msg)
        {
            ConsoleColor log = Console.ForegroundColor;
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
            }

            Console.WriteLine($"{DateTime.Now,-19} [{msg.Severity,8}] {msg.Source}: {msg.Message}");
                Console.ForegroundColor = log;

            return Task.CompletedTask;
        }

        internal static Task Start(object workingDirectly, object friendlyName)
        {
            throw new NotImplementedException();
        }

        internal static Task Start(string v)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
