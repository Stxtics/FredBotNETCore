using Newtonsoft.Json;
using System.Collections.Generic;

namespace FredBotNETCore.Models
{
    public class PR2LevelSearchResponse
    {
        public PR2LevelSearchResponse(List<PR2Level> levels = null, string error = null)
        {
            Levels = levels;
            Error = error;
        }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("levels")]
        public List<PR2Level> Levels { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }
    }
}