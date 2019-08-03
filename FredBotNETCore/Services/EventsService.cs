using Discord;
using Discord.WebSocket;
using FredBotNETCore.Database;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FredBotNETCore.Services
{
    public class EventsService
    {
        private readonly DiscordSocketClient _client;

        public EventsService(DiscordSocketClient client)
        {
            _client = client;
        }

        public void SetupEvents()
        {
            if (_client.LoginState != LoginState.LoggedIn)
            {
                return;
            }

            _client.Ready += OnReady;

            ActionLog log = new ActionLog();

            _client.MessageUpdated += OnMessageEdited;
            _client.UserJoined += log.AnnounceUserJoined;
            _client.UserLeft += log.AnnounceUserLeft;
            _client.UserBanned += log.AnnounceUserBanned;
            _client.UserUnbanned += log.AnnounceUserUnbanned;
            _client.MessageDeleted += log.AnnounceMessageDeleted;
            _client.MessagesBulkDeleted += log.AnnounceBulkDelete;
            _client.GuildMemberUpdated += log.AnnounceGuildMemberUpdated;
            _client.ChannelCreated += log.AnnounceChannelCreated;
            _client.ChannelDestroyed += log.AnnounceChannelDestroyed;
            _client.ChannelUpdated += log.AnnounceChannelUpdated;
            _client.RoleCreated += log.AnnounceRoleCreated;
            _client.RoleDeleted += log.AnnounceRoleDeleted;
            _client.RoleUpdated += log.AnnounceRoleUpdated;
            _client.JoinedGuild += log.OnGuildJoin;
        }

        private async Task OnReady()
        {
            int users = _client.Guilds.Sum(g => g.MemberCount);
            await _client.SetGameAsync($"/help with {users} users", null, type: ActivityType.Listening);
            await _client.DownloadUsersAsync(_client.Guilds);
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
                if (Extensions.GetLogChannel(channel.Guild) != null && channel != Extensions.GetLogChannel(channel.Guild))
                {
                    if (DiscordStaff.Get(channel.Guild.Id, "u-" + msg.Author.Id).Count <= 0 || (channel.Guild.GetUser(msg.Author.Id).Roles.Count > 1 && DiscordStaff.Get(channel.Guild.Id, "r-" + channel.Guild.GetUser(msg.Author.Id).Roles.Where(x => x.IsEveryone == false).OrderBy(x => x.Position).First().Id).Count > 0))
                    {
                        AutoMod mod = new AutoMod(_client);
                        await mod.FilterMessage(msg, channel);
                    }
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
                await _client.DownloadUsersAsync(_client.Guilds);
            }
        }
    }
}
