using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FredBotNETCore.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Victoria;

namespace FredBotNETCore
{
    public class CommandHandler
    {
        private CommandService _cmds;
        public static DiscordSocketClient _client;
        public static Lavalink _lavaLink;

        public static string Name;

        public static string hint;

        public static string CheckHint
        {
            get => hint;
            set
            {
                if (value == hint)
                {
                    return;
                }

                if (value.Contains("\"happy_hour\":\"1\""))
                {
                    hint = value;
                }
            }
        }

        public ServiceProvider Services { get; set; }

        public static bool justConnected;

        public static string DerronStatus = "";
        public static string CarinaStatus = "";
        public static string GrayanStatus = "";
        public static string FitzStatus = "";
        public static string LokiStatus = "";
        public static string PromieStatus = "";
        public static string MorganaStatus = "";
        public static string AndresStatus = "";
        public static string IsabelStatus = "";

        public static async Task CheckStatusAsync(bool isOn = false, string serverName = null)
        {
            string compare = isOn + serverName;
            switch (serverName)
            {
                case "Derron":
                    if (DerronStatus == compare)
                    {
                        return;
                    }

                    DerronStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Carina":
                    if (CarinaStatus == compare)
                    {
                        return;
                    }

                    CarinaStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Grayan":
                    if (GrayanStatus == compare)
                    {
                        return;
                    }

                    GrayanStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Fitz":
                    if (FitzStatus == compare)
                    {
                        return;
                    }

                    FitzStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Loki":
                    if (LokiStatus == compare)
                    {
                        return;
                    }

                    LokiStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Promie":
                    if (PromieStatus == compare)
                    {
                        return;
                    }

                    PromieStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Morgana":
                    if (MorganaStatus == compare)
                    {
                        return;
                    }

                    MorganaStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Andres":
                    if (AndresStatus == compare)
                    {
                        return;
                    }

                    AndresStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Isabel":
                    if (IsabelStatus == compare)
                    {
                        return;
                    }

                    IsabelStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;
            }
        }

        private static async Task UpdateHappyHourAsync(string Name = null, bool isOn = false)
        {
            if (isOn)
            {
                Process process = Process.GetCurrentProcess();
                TimeSpan time = DateTime.Now - process.StartTime;
                if (time.Minutes < 2)
                {
                    return;
                }
                SocketGuild Guild = _client.GetGuild(528679522707701760);
                SocketRole RoleM = Guild.Roles.Where(x => x.Name.ToUpper() == "HH".ToUpper()).First();
                SocketTextChannel channel = Guild.GetTextChannel(Extensions.GetNotificationsChannel());
                RequestOptions options = new RequestOptions()
                {
                    AuditLogReason = "Announcing happy hour on " + Name
                };
                await RoleM.ModifyAsync(x => x.Mentionable = true, options);
                await channel.SendMessageAsync($"{RoleM.Mention} A happy hour has just started on Server: {Name}");
                await RoleM.ModifyAsync(x => x.Mentionable = false, options);
            }
        }

        public static async Task AnnouceHintUpdatedAsync(string hint = null, bool newArti = false)
        {
            SocketGuild Guild = _client.GetGuild(528679522707701760);
            SocketTextChannel channel = Guild.GetTextChannel(Extensions.GetNotificationsChannel());
            RequestOptions options = new RequestOptions()
            {
                AuditLogReason = "Announcing new artifact"
            };
            if (newArti)
            {
                await Guild.Roles.Where(x => x.Name.ToUpper() == "Arti".ToUpper()).First().ModifyAsync(x => x.Mentionable = true, options);
                await channel.SendMessageAsync($"{Guild.Roles.Where(x => x.Name.ToUpper() == "Arti".ToUpper()).First().Mention} Hmm... I seem to have misplaced the artifact. Maybe you can help me find it?\n" +
                        $"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(hint))}**. Maybe I can remember more later!!");
                await Guild.Roles.Where(x => x.Name.ToUpper() == "Arti".ToUpper()).First().ModifyAsync(x => x.Mentionable = false, options);
            }
            else
            {
                await channel.SendMessageAsync($"Artifact hint updated. New hint: **{Format.Sanitize(Uri.UnescapeDataString(hint))}**");
            }
        }

        public static async Task AnnounceArtifactFoundAsync(string finder = null)
        {
            SocketGuild Guild = _client.GetGuild(528679522707701760);
            SocketTextChannel channel = Guild.GetTextChannel(Extensions.GetNotificationsChannel());
            await channel.SendMessageAsync($"**{finder}** has found the artifact!");
        }

        public async Task Install(DiscordSocketClient c, Lavalink lavalink, ServiceProvider provider)
        {

            if (c.LoginState != LoginState.LoggedIn)
            {
                return;
            }
            _lavaLink = lavalink;
            Services = provider;
            _client = c;
            _cmds = new CommandService();
            await _cmds.AddModulesAsync(Assembly.GetEntryAssembly(), Program._provider);
            _cmds.Log += LogException;
            ActionLog log = new ActionLog(_client);
            _client.MessageReceived += OnMessageReceived;
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
            _client.Ready += OnReady;
        }

        private async Task OnReady()
        {
            await _client.DownloadUsersAsync(_client.Guilds);
            int users = _client.Guilds.Sum(g => g.Users.Count);
            await _client.SetGameAsync($"/help with {users} users", null, type: ActivityType.Listening);
            LavaNode node = await _lavaLink.AddNodeAsync(_client, new Configuration
            {
                Severity = LogSeverity.Info
            }).ConfigureAwait(false);
            node.TrackFinished += AudioService.OnFinished;
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
            if (message2.Content != msg.Content && msg.Channel is SocketGuildChannel && msg.Channel is SocketTextChannel channel)
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

            if (msg.Channel is SocketGuildChannel && msg.Channel is SocketTextChannel channel)
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
                IResult result = await _cmds.ExecuteAsync(context, argPos, Program._provider);
                if (result.Error.HasValue && result.Error.Value == CommandError.Exception)
                {
                    await context.Channel.SendMessageAsync($"Oh no an error occurred. Details of this error have been sent to {(await _client.GetApplicationInfoAsync()).Owner.Mention} so that he can fix it.");
                }
            }
        }
    }
}
