using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace FredBotNETCore.Services
{
    public class StartupService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;

        public StartupService(DiscordSocketClient client, CommandService commands, IServiceProvider provider)
        {
            _client = client;
            _commands = commands;
            _provider = provider;
        }

        public async Task StartAsync()
        {
            try
            {
                string token = File.ReadAllText(Path.Combine(Extensions.downloadPath, "Token.txt"));
                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();
            }
            catch (FileNotFoundException)
            {
                await Log(new LogMessage(LogSeverity.Error, "RunAsync", "Token file not found."));
                await Task.Delay(1000);
                Environment.Exit(0);
            }
            catch(Exception ex)
            {
                await Log(new LogMessage(LogSeverity.Error, "RunAsync", "Failed to connect."));
                await Task.Delay(1000);
                Environment.Exit(0);
            }

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

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
    }
}
