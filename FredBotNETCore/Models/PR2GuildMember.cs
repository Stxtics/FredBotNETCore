using Newtonsoft.Json;

namespace FredBotNETCore.Models
{
    public class PR2GuildMember
    {
        [JsonProperty("user_id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("power")]
        public int Power { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("gp_today")]
        public int? GPToday { get; set; }

        [JsonProperty("gp_total")]
        public int? GPTotal { get; set; }

        public bool IsOwner { get; set; } = false;
    }
}
