using Newtonsoft.Json;

namespace FredBotNETCore.Models
{
    public class PR2Hint
    {
        [JsonProperty("hint")]
        public string Hint { get; set; }

        [JsonProperty("finder_name")]
        public string FinderName { get; set; }

        [JsonProperty("bubbles_name")]
        public string BubblesName { get; set; }

        [JsonProperty("updated_time")]
        public int UpdatedTime { get; set; }
    }
}