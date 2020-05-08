using Newtonsoft.Json;

namespace FredBotNETCore.Models
{
    public class PR2PM
    {
        [JsonProperty("message_id")]
        public int MessageId { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("time")]
        public int Time { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("group")]
        public int Group { get; set; }

        [JsonProperty("guild_message")]
        public bool GuildMessage { get; set; }
    }
}