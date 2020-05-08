using Newtonsoft.Json;
using System.Collections.Generic;

namespace FredBotNETCore.Models
{
    public class PR2PMList
    {
        public PR2PMList(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("messages")]
        public List<PR2PM> Messages { get; set; }
    }
}