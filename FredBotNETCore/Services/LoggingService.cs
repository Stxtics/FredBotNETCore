using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Victoria;

namespace FredBotNETCore.Services
{
    public class LoggingService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly Lavalink _lavalink;

        public LoggingService(DiscordSocketClient client, CommandService commands, Lavalink lavalink)
        {
            _client = client;
            _commands = commands;
            _lavalink = lavalink;

            _client.Log += Log;
            _commands.Log += LogException;
            _lavalink.Log += Log;
            //AppDomain.CurrentDomain.FirstChanceException += async (sender, eventArgs) =>
            //{
            //    SocketUser user = _client.GetUser(181853112045142016);
            //    if (eventArgs.Exception.ToString().Contains("The SSL connection could not be established"))
            //    {
            //        return;
            //    }
            //    System.Collections.Generic.IEnumerable<string> parts = eventArgs.Exception.ToString().SplitInParts(1950);
            //    bool first = true;
            //    foreach (string part in parts)
            //    {
            //        if (first)
            //        {
            //            await user.SendMessageAsync("Global Catch\n```" + part + "```");
            //            first = false;
            //        }
            //        else
            //        {
            //            await user.SendMessageAsync("```" + part + "```");
            //        }
            //    }
            //};
        }

        private async Task LogException(LogMessage message)
        {
            if (!message.Equals(null) && message.Exception != null)
            {
                SocketUser user = _client.GetUser(181853112045142016);
                System.Collections.Generic.IEnumerable<string> parts = message.Exception.ToString().SplitInParts(1950);
                foreach (string part in parts)
                {
                    await user.SendMessageAsync("Command Catch\n```" + part + "```");
                }
            }
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
