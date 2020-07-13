using Newtonsoft.Json;

namespace FredBotNETCore.Models
{
    public class PR2Guild
    {
        public PR2Guild(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("guild_id")]
        public int Id { get; set; }

        [JsonProperty("guild_name")]
        public string Name { get; set; }

        [JsonProperty("creation_date")]
        public string CreationDate { get; set; }

        [JsonProperty("active_date")]
        public string ActiveDate { get; set; }

        [JsonProperty("member_count")]
        public int MemberCount { get; set; }

        [JsonProperty("emblem")]
        public string Emblem { get; set; }

        [JsonProperty("gp_total")]
        public int GPTotal { get; set; }

        [JsonProperty("gp_today")]
        public int GPToday { get; set; }

        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("active_count")]
        public int ActiveCount { get; set; }
    }
}