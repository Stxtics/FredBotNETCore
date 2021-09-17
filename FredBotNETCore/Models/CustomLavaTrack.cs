using Discord.WebSocket;
using Victoria;

namespace FredBotNETCore.Models
{
    public class CustomLavaTrack : LavaTrack
    {
        /// <summary>
        /// The user who queued the track.
        /// </summary>
        public SocketUser User { get; set; }

        public CustomLavaTrack(LavaTrack track, SocketUser user) : base(track)
        {
            User = user;
        }
    }
}
