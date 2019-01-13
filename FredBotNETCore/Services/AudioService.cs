using Discord;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.Entities;
using Victoria.Entities.Enums;

namespace FredBotNETCore.Services
{
    public class AudioService
    {
        private LavaNode _node;
        private LavaPlayer _player;

        public void Initialize(LavaNode node)
        {
            _node = node;
            _node.TrackFinished = OnFinished;
        }

        public async Task Connect(IVoiceChannel voiceChannel, IMessageChannel messageChannel)
        {
            _player = await _node.ConnectAsync(voiceChannel, messageChannel);
        }

        public async Task Disconnect(ulong guildId)
        {
            await _node.DisconnectAsync(guildId);
        }

        private Task OnFinished(LavaPlayer player, LavaTrack track, TrackReason reason)
        {
            return Task.CompletedTask;
        }

        public async Task Play(LavaTrack track)
        {
            await _player.PlayAsync(track);
        }

        public async Task<LavaTrack> GetTrack(string searchQuery)
        {
            LavaResult search = await _node.GetTracksAsync(searchQuery);
            return search.Tracks.FirstOrDefault();
        }

        public async Task Pause()
        {
            await _player.PauseAsync();
        }

        public async Task Resume()
        {
            await _player.PauseAsync();
        }

        public async Task SetVolume(int volume)
        {
            await _player.SetVolumeAsync(volume);
        }

        public async Task Skip()
        {
            await _player.SkipAsync();
        }

        public LavaTrack NowPlaying()
        {
            return _player.CurrentTrack;
        }

        public async Task Stop()
        {
            await _player.StopAsync();
        }

        public bool Paused()
        {
            return _player.IsPaused;
        }

        public bool Playing()
        {
            return _player.IsPlaying;
        }

        public int GetVolume()
        {
            return _player.Volume;
        }
        
        public LavaQueue<LavaTrack> Queue()
        {
            if (_player.Queue != null)
            {
                return _player.Queue;
            }
            return null;
        }

        public void QueueAdd(LavaTrack track)
        {
            _player.Queue.Enqueue(track);
        }

        public void QueueRemove(int index)
        {
            _player.Queue.RemoveAt(index);
        }

        public void QueueClear()
        {
            _player.Queue.Clear();
        }
    }
}
