using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Victoria;

namespace FredBotNETCore.Models
{
    public class CustomLavaPlayer : LavaPlayer
    {
        /// <summary>
        /// The user who queued the current Track.
        /// </summary>
        public SocketUser User
        {
            get;
            private set;
        }

        /// <summary>
        /// The queue of CustomLavaTracks.
        /// </summary>
        public DefaultQueue<CustomLavaTrack> CustomQueue
        {
            get;
            private set;
        }

        /// <summary>
        /// The users who have voted to skip the current track.
        /// </summary>
        public List<SocketUser> SkippedUsers { get; set; } = new List<SocketUser>();

        /// <summary>
        /// Whether the queue should loop.
        /// </summary>
        public bool QueueLoop { get; set; } = false;

        /// <summary>
        /// The time that the last song finished.
        /// </summary>
        public DateTime LastFinishTime { get; set; }

        public CustomLavaPlayer(LavaSocket lavaSocket, IVoiceChannel voiceChannel, ITextChannel textChannel) : base(lavaSocket, voiceChannel, textChannel)
        {
            CustomQueue = new DefaultQueue<CustomLavaTrack>();
            LastFinishTime = DateTime.Now;
        }

        public async Task PlayAsync(CustomLavaTrack lavaTrack)
        {
            User = lavaTrack.User;
            await base.PlayAsync(lavaTrack);
        }

        public new async Task StopAsync()
        {
            await base.StopAsync();
            User = null;
        }

        public new async Task<(CustomLavaTrack Skipped, CustomLavaTrack Current)> SkipAsync(TimeSpan? delay = null)
        {
            LavaTrack skipped = (await base.SkipAsync(delay)).Skipped;
            CustomQueue.TryDequeue(out CustomLavaTrack lavaTrack);

            CustomLavaTrack customSkipped = new CustomLavaTrack(skipped, User);

            User = lavaTrack.User;

            return (customSkipped, lavaTrack);
        }
    }
}
