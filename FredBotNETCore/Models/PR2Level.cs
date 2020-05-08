using Newtonsoft.Json;

namespace FredBotNETCore.Models
{
    public class PR2Level
    {
        public PR2Level(bool success, string error = null)
        {
            Success = success;
            Error = error;
        }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("level_id")]
        public int Id { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("credits")]
        public string Credits { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("time")]
        public int Time { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("min_rank")]
        public int MinLevel { get; set; }

        [JsonProperty("song")]
        public string Song { get; set; }

        [JsonProperty("gravity")]
        public double Gravity { get; set; }

        [JsonProperty("max_time")]
        public int MaxTime { get; set; }

        [JsonProperty("has_pass")]
        public bool HasPass { get; set; }

        [JsonProperty("live")]
        public bool Live { get; set; }

        [JsonProperty("items")]
        public string Items { get; set; }

        [JsonProperty("gameMode")]
        public string GameMode { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("cowboyChance")]
        public double CowboyChance { get; set; }

        [JsonProperty("user_name")]
        public string Username { get; set; }

        [JsonProperty("user_group")]
        public int UserGroup { get; set; }

        [JsonProperty("play_count")]
        public int PlayCount { get; set; }
    }
}
