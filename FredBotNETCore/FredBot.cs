using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FredBotNETCore.Models;
using FredBotNETCore.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Victoria;

namespace FredBotNETCore
{
    public class FredBot
    {
        public async Task RunAsync()
        {
            IServiceProvider services = InstallServices();

            services.GetRequiredService<LoggingService>();

            await services.GetRequiredService<StartupService>().StartAsync();

            services.GetRequiredService<CommandHandler>();

            Task.WaitAny(Task.Factory.StartNew(() => services.GetRequiredService<EventsService>().SetupEvents()), Task.Factory.StartNew(() => services.GetRequiredService<EventsService>().GameLoop()), Task.Factory.StartNew(() => services.GetRequiredService<NotificationsHandler>().CheckStatus()));

            await Task.Delay(-1);
        }

        private IServiceProvider InstallServices()
        {
            IServiceCollection services = new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 100,
                    ExclusiveBulkDelete = true
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Verbose,
                    CaseSensitiveCommands = false,
                    ThrowOnError = false
                }))
                .AddSingleton<AdminService>()
                .AddSingleton<AudioService>()
                .AddSingleton<ModeratorService>()
                .AddSingleton<PublicService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<StartupService>()
                .AddSingleton<EventsService>()
                .AddSingleton<NotificationsHandler>()
                .AddSingleton<LoggingService>()
                .AddLavaNode<CustomLavaPlayer>(x =>
                {
                    x.SelfDeaf = true;
                });

            return services.BuildServiceProvider();
        }
    }
}
