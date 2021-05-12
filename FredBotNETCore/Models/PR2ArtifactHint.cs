using Newtonsoft.Json;

namespace FredBotNETCore.Models
{
    public partial class PR2ArtifactHint
    {
        [JsonProperty("current")]
        public Current Current { get; set; }

        [JsonProperty("scheduled")]
        public Scheduled Scheduled { get; set; }
    }

    public partial class Current
    {
        [JsonProperty("level")]
        public Level Level { get; set; }

        [JsonProperty("set_time")]
        public long SetTime { get; set; }

        [JsonProperty("updated_time")]
        public long UpdatedTime { get; set; }

        [JsonProperty("first_finder")]
        public PR2User FirstFinder { get; set; }

        [JsonProperty("bubbles_winner")]
        public PR2User BubblesWinner { get; set; }
    }

    public partial class Level
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("author")]
        public PR2User Author { get; set; }
    }

    public partial class Scheduled
    {
        [JsonProperty("level")]
        public Level Level { get; set; }

        [JsonProperty("set_time")]
        public long SetTime { get; set; }

        [JsonProperty("updated_time")]
        public long UpdatedTime { get; set; }
    }
}
