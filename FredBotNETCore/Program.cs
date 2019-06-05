using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FredBotNETCore.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Victoria;

namespace FredBotNETCore
{
    public class Program
    {
        private DiscordSocketClient _client;
        private CommandService _cmds;
        private IServiceProvider _services;
        private bool _retryConnection;
        private bool _running;
        public static Lavalink _lavaLink;

        private static readonly System.Threading.Mutex INSTANCE_MUTEX = new System.Threading.Mutex(true, "FredBotNETCore");

        private static void Main(string[] args)
        {
            if (!INSTANCE_MUTEX.WaitOne(TimeSpan.Zero, false))
            {
                Console.WriteLine("Already running");
                return;
            }
            Program app = new Program();
            app.RunAsync().GetAwaiter().GetResult();
        }
        public async Task RunAsync()
        {
            if (_client != null)
            {
                if (_client.ConnectionState == ConnectionState.Connecting || _client.ConnectionState == ConnectionState.Connected)
                {
                    return;
                }
            }

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 100,
                LogLevel = LogSeverity.Verbose
            });

            _cmds = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async
            });

            _services = InstallServices();
            _lavaLink = _services.GetRequiredService<Lavalink>();
            _lavaLink.Log += Log;
            
            _retryConnection = true;
            _running = false;

            while (true)
            {
                try
                {
                    string token = File.ReadAllText(Path.Combine(Extensions.downloadPath, "Token.txt"));
                    await _client.LoginAsync(TokenType.Bot, token);
                    await _client.StartAsync();


                    Task.WaitAny(Task.Factory.StartNew(() => NotificationsHandler.CheckStatus(_client)), Task.Factory.StartNew(() => GameLoop()), Task.Factory.StartNew(() => InstallCommands()));

                    _running = true;

                    break;
                }
                catch (FileNotFoundException)
                {
                    await Log(new LogMessage(LogSeverity.Error, "RunAsync", "Token file not found."));
                    if (_retryConnection == false)
                    {
                        return;
                    }
                    await Task.Delay(1000);
                }
                catch
                {
                    await Log(new LogMessage(LogSeverity.Error, "RunAsync", "Failed to connect."));
                    if (_retryConnection == false)
                    {
                        return;
                    }
                    await Task.Delay(1000);
                }
            }

            while (_running) { await Task.Delay(1000); }

            if (_client.ConnectionState == ConnectionState.Connecting || _client.ConnectionState == ConnectionState.Connected)
            {
                try { _client.StopAsync().Wait(); }
                catch { }
            }
        }

        public async Task CancelAsync()
        {
            _retryConnection = false;
            await Task.Delay(0);
        }

        public async Task StopAsync()
        {
            if (_running)
            {
                _running = false;
            }

            await Task.Delay(0);
        }

        private IServiceProvider InstallServices()
        {
            ServiceCollection services = new ServiceCollection();

            services.AddSingleton<AdminService>();
            services.AddSingleton<AudioService>();
            services.AddSingleton<ModeratorService>();
            services.AddSingleton<PublicService>();
            services.AddSingleton<Lavalink>();

            return services.BuildServiceProvider();
        }

        private async Task InstallCommands()
        {

            if (_client.LoginState != LoginState.LoggedIn)
            {
                return;
            }

            _cmds.Log += LogException;

            _client.MessageReceived += OnMessageReceived;

            _client.Ready += OnReady;

            ActionLog log = new ActionLog(_client);

            _client.MessageUpdated += OnMessageEdited;
            _client.UserJoined += log.AnnounceUserJoined;
            _client.UserLeft += log.AnnounceUserLeft;
            _client.UserBanned += log.AnnounceUserBanned;
            _client.UserUnbanned += log.AnnounceUserUnbanned;
            _client.MessageDeleted += log.AnnounceMessageDeleted;
            _client.GuildMemberUpdated += log.AnnounceGuildMemberUpdated;
            _client.ChannelCreated += log.AnnounceChannelCreated;
            _client.ChannelDestroyed += log.AnnounceChannelDestroyed;
            _client.ChannelUpdated += log.AnnounceChannelUpdated;
            _client.RoleCreated += log.AnnounceRoleCreated;
            _client.RoleDeleted += log.AnnounceRoleDeleted;
            _client.RoleUpdated += log.AnnounceRoleUpdated;
            _client.JoinedGuild += log.OnGuildJoin;
            _client.Log += Log;

            LavaNode node = await _lavaLink.AddNodeAsync(_client, new Configuration
            {
                Severity = LogSeverity.Info
            }).ConfigureAwait(false);
            node.TrackFinished += AudioService.OnFinished;

            await _cmds.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task OnReady()
        {
            int users = _client.Guilds.Sum(g => g.MemberCount);
            await _client.SetGameAsync($"/help with {users} users", null, type: ActivityType.Listening);
        }


        public async Task LogException(LogMessage message)
        {
            SocketUser user = _client.GetUser(181853112045142016);
            System.Collections.Generic.IEnumerable<string> parts = message.Exception.ToString().SplitInParts(1990);
            foreach (string part in parts)
            {
                await user.SendMessageAsync("```" + part + "```");
            }
        }

        public async Task OnMessageEdited(Cacheable<IMessage, ulong> message, SocketMessage m, ISocketMessageChannel chl)
        {
            if (!(m is SocketUserMessage msg))
            {
                return;
            }
            if (msg.Author.IsBot)
            {
                return;
            }
            IMessage message2 = await message.GetOrDownloadAsync();
            if (message2.Content != msg.Content && msg.Channel is SocketTextChannel channel)
            {
                if (channel.Guild.Id == 528679522707701760 && channel.Id != Extensions.GetLogChannel())
                {
                    if (!Extensions.CheckStaff(msg.Author.Id.ToString(), channel.Guild.GetUser(msg.Author.Id).Roles.Where(x => x.IsEveryone == false)))
                    {
                        AutoMod mod = new AutoMod(_client);
                        await mod.FilterMessage(msg, channel);
                    }
                }
            }
        }

        public async Task OnMessageReceived(SocketMessage m)
        {
            if (!(m is SocketUserMessage msg))
            {
                return;
            }

            bool badMessage = false;
            if (msg.Author.IsBot)
            {
                return;
            }

            if (msg.Channel is SocketTextChannel channel)
            {
                if (channel.Guild.Id == 528679522707701760 && channel.Id != Extensions.GetLogChannel())
                {
                    if (!Extensions.CheckStaff(msg.Author.Id.ToString(), channel.Guild.GetUser(msg.Author.Id).Roles.Where(x => x.IsEveryone == false)))
                    {
                        AutoMod mod = new AutoMod(_client);
                        badMessage = await mod.FilterMessage(msg, channel);
                    }
                }
            }
            if (!badMessage)
            {
                await HandleCommand(msg);
            }
        }

        public async Task HandleCommand(SocketUserMessage msg)
        {
            SocketCommandContext context = new SocketCommandContext(_client, msg);
            int argPos = 0;
            if (msg.HasStringPrefix("/", ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                if (context.Channel is SocketTextChannel channel)
                {
                    if (channel.Guild.CurrentUser.GetPermissions(channel).SendMessages == false)
                    {
                        return;
                    }
                    else if (channel.Guild.CurrentUser.GetPermissions(channel).EmbedLinks == false)
                    {
                        await channel.SendMessageAsync("Error: I am missing permission **Embed Links**.");
                        return;
                    }
                }
                IResult result = await _cmds.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess || result.Error.HasValue && result.Error.Value == CommandError.Exception)
                {
                    await context.Channel.SendMessageAsync($"Oh no an error occurred. Details of this error have been sent to **{(await _client.GetApplicationInfoAsync()).Owner.Username}#{(await _client.GetApplicationInfoAsync()).Owner.Discriminator}** so that he can fix it.");
                }
            }
        }

        public async Task GameLoop()
        {
            while (true)
            {
                await Task.Delay(new Random().Next(300000, 600000));
                Process process = Process.GetCurrentProcess();
                TimeSpan time = DateTime.Now - process.StartTime;
                StringBuilder sb = new StringBuilder();
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
                int users = _client.Guilds.Sum(g => g.MemberCount);
                await _client.SetGameAsync($"/help with {users} users", null, type: ActivityType.Listening);
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