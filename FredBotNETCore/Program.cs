using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

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

            await CheckStatus();
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
                catch (Exception e)
                {
                    await CommandHandler.SendError(e.Message, e.StackTrace);
                }
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
