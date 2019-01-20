using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.Entities;
using Victoria.Entities.Enums;

namespace FredBotNETCore.Services
{
    public class AudioService
    {
        private readonly Lavalink _lavalink;
        private static bool QueueLoop { get; set; } = false;
        private static List<SocketUser> SkippedUsers { get; set; } = new List<SocketUser>();
        private static List<SocketUser> UserQueue { get; set; } = new List<SocketUser>();
        private static SocketUser NowPlayingUser { get; set; } = null;

        public AudioService(Lavalink lavalink)
        {
            _lavalink = lavalink;
        }

        public async Task Connect(IVoiceChannel voiceChannel, IMessageChannel messageChannel)
        {
            _lavalink.DefaultNode.TrackFinished += OnFinished;
            await _lavalink.DefaultNode.ConnectAsync(voiceChannel, messageChannel);
        }

        public async Task Disconnect(ulong guildId)
        {
            await _lavalink.DefaultNode.DisconnectAsync(guildId);
        }

        private async Task OnFinished(LavaPlayer player, LavaTrack track, TrackReason reason)
        {
            if (reason is TrackReason.LoadFailed || reason is TrackReason.Cleanup || reason is TrackReason.Replaced)
            {
                return;
            }
            player.Queue.TryDequeue(out LavaTrack nextTrack);
            if (nextTrack is null && !QueueLoop)
            {
                await player.TextChannel.SendMessageAsync("Queue finished.");
                await Disconnect(player.VoiceChannel.Guild.Id);
                SkippedUsers.Clear();
                UserQueue.Clear();
                NowPlayingUser = null;
            }
            else if (QueueLoop)
            {
                SkippedUsers.Clear();
                if (nextTrack is null)
                {
                    await player.PlayAsync(track);
                    await player.TextChannel.SendMessageAsync($"Now playing: **{Format.Sanitize(track.Title)}** ({track.Length.Minutes}:{track.Length.Seconds.ToString("D2")}) requested by **{NowPlayingUser.Username}#{NowPlayingUser.Discriminator}**.");
                }
                else
                {
                    player.Queue.Enqueue(track);
                    UserQueue.Add(NowPlayingUser);
                    SocketUser nextUser = UserQueue.FirstOrDefault();
                    UserQueue.RemoveAt(0);
                    NowPlayingUser = nextUser;
                    await player.PlayAsync(nextTrack);
                    await player.TextChannel.SendMessageAsync($"Now playing: **{Format.Sanitize(nextTrack.Title)}** ({nextTrack.Length.Minutes}:{nextTrack.Length.Seconds.ToString("D2")}) requested by **{nextUser.Username}#{nextUser.Discriminator}**.");
                }
            }
            else
            {
                SkippedUsers.Clear();
                SocketUser nextUser = UserQueue.FirstOrDefault();
                UserQueue.RemoveAt(0);
                NowPlayingUser = nextUser;
                await player.PlayAsync(nextTrack);
                await player.TextChannel.SendMessageAsync($"Now playing: **{Format.Sanitize(nextTrack.Title)}** ({nextTrack.Length.Minutes}:{nextTrack.Length.Seconds.ToString("D2")}) requested by **{nextUser.Username}#{nextUser.Discriminator}**.");
            }
        }

        public async Task Play(ulong guildId, LavaTrack track)
        {
            LavaPlayer player = _lavalink.DefaultNode.GetPlayer(guildId);
            await player.PlayAsync(track);
        }

        public async Task<LavaTrack> GetTrackFromYoutube(string searchQuery)
        {
            LavaResult search = await _lavalink.DefaultNode.SearchYouTubeAsync(searchQuery);
            if (search.LoadResultType == LoadResultType.NoMatches)
            {
                return null;
            }
            return search.Tracks.FirstOrDefault();
        }

        public async Task Pause(ulong guildId)
        {
            await _lavalink.DefaultNode.GetPlayer(guildId).PauseAsync();
        }

        public async Task Resume(ulong guildId)
        {
            await _lavalink.DefaultNode.GetPlayer(guildId).PauseAsync();
        }

        public async Task SetVolume(ulong guildId, int volume)
        {
            await _lavalink.DefaultNode.GetPlayer(guildId).SetVolumeAsync(volume);
        }

        public async Task Skip(ulong guildId)
        {
            if (_lavalink.DefaultNode.GetPlayer(guildId).Queue.Count == 0)
            {
                await _lavalink.DefaultNode.GetPlayer(guildId).StopAsync();
                UserQueue.Clear();
                SkippedUsers.Clear();
                NowPlayingUser = null;
            }
            else
            {
                LavaTrack nextTrack = _lavalink.DefaultNode.GetPlayer(guildId).Queue.Peek();
                await _lavalink.DefaultNode.GetPlayer(guildId).SkipAsync();
                SocketUser user = UserQueue.FirstOrDefault();
                UserQueue.RemoveAt(0);
                SkippedUsers.Clear();
                if (UserQueue.Count == 0)
                {
                    NowPlayingUser = user;
                }
                else
                {
                    SocketUser nextUser = UserQueue.FirstOrDefault();
                    NowPlayingUser = nextUser;
                }
                await _lavalink.DefaultNode.GetPlayer(guildId).TextChannel.SendMessageAsync($"Now playing: **{Format.Sanitize(nextTrack.Title)}** ({nextTrack.Length.Minutes}:{nextTrack.Length.Seconds.ToString("D2")}) requested by **{NowPlayingUser.Username}#{NowPlayingUser.Discriminator}**.");
            }
        }

        public List<SocketUser> GetSkippedUsers()
        {
            return SkippedUsers;
        }

        public void AddSkippedUser(SocketUser user)
        {
            SkippedUsers.Add(user);
        }

        public LavaTrack NowPlaying(ulong guildId)
        {
            return _lavalink.DefaultNode.GetPlayer(guildId).CurrentTrack;
        }

        public async Task Stop(ulong guildId)
        {
            LavaPlayer player = _lavalink.DefaultNode.GetPlayer(guildId);
            if (player.IsPlaying)
            {
                await player.StopAsync();
            }
            await Disconnect(guildId);
            UserQueue.Clear();
            SkippedUsers.Clear();
            NowPlayingUser = null;
        }

        public bool Paused(ulong guildId)
        {
            return _lavalink.DefaultNode.GetPlayer(guildId).IsPaused;
        }

        public bool Playing(ulong guildId)
        {
            return _lavalink.DefaultNode.GetPlayer(guildId).IsPlaying;
        }

        public int GetVolume(ulong guildId)
        {
            return _lavalink.DefaultNode.GetPlayer(guildId).Volume;
        }

        public Tuple<LavaQueue<LavaTrack>, List<SocketUser>> Queue(ulong guildId)
        {
            if (_lavalink.DefaultNode.GetPlayer(guildId).Queue != null)
            {
                return Tuple.Create(_lavalink.DefaultNode.GetPlayer(guildId).Queue, UserQueue);
            }
            return null;
        }

        public void QueueAdd(ulong guildId, LavaTrack track, SocketUser user)
        {
            _lavalink.DefaultNode.GetPlayer(guildId).Queue.Enqueue(track);
            UserQueue.Add(user);
        }

        public Tuple<LavaTrack, SocketUser> QueueRemove(ulong guildId, int index)
        {
            SocketUser user = UserQueue[index];
            UserQueue.RemoveAt(index);
            return Tuple.Create(_lavalink.DefaultNode.GetPlayer(guildId).Queue.RemoveAt(index), user);
        }

        public void QueueClear(ulong guildId)
        {
            _lavalink.DefaultNode.GetPlayer(guildId).Queue.Clear();
            UserQueue.Clear();
        }

        public void SetNowPlayingUser(SocketUser user)
        {
            NowPlayingUser = user;
        }

        public SocketUser GetNowPlayingUser()
        {
            return NowPlayingUser;
        }

        public bool Loop()
        {
            if (QueueLoop)
            {
                QueueLoop = false;
            }
            else
            {
                QueueLoop = true;
            }
            return QueueLoop;
        }
    }
}
