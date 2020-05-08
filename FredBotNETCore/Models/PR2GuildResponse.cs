using Newtonsoft.Json;
using System.Collections.Generic;

namespace FredBotNETCore.Models
{
    public class PR2GuildResponse
    {
        public PR2GuildResponse(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("guild")]
        public PR2Guild Guild { get; set; }

        [JsonProperty("Members")]
        private List<PR2GuildMember> Members
        {
            get => Guild.Members;
            set => Guild.Members = value;
        }
    }
}