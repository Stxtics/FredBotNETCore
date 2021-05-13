using Newtonsoft.Json;
using System;

namespace FredBotNETCore.Models
{
    public class PR2User
    {
        public PR2User(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("userId")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("group")]
        public string Group
        {
            get
            {
                if (_group == "0")
                {
                    _group = "Guest";
                }
                else if (_group == "1")
                {
                    _group = "Member";
                }
                else if (_group == "2")
                {
                    _group = "[Moderator](https://pr2hub.com/staff.php)";
                }
                else if (_group == "3")
                {
                    _group = "[Admin](https://pr2hub.com/staff.php)";
                }
                return _group;
            }
            set
            {
                _group = value;
            }
        }
        private string _group;

        [JsonProperty("trial_mod")]
        public bool TrailMod { get; set; }

        [JsonProperty("guildId")]
        public string GuildId { get; set; }

        [JsonProperty("guildName")]
        public string GuildName { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("hats")]
        public int Hats { get; set; }

        [JsonProperty("registerDate")]
        public long RegisterDateEpoch;

        public string RegisterDate
        {
            get
            {
                if (RegisterDateEpoch == 0)
                {
                    return "Age of Heroes";
                }
                else
                {
                    DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(RegisterDateEpoch);
                    return  $"{date.Day}/{date:MMM}/{date.Year}";
                }
            }
        }

        [JsonProperty("loginDate")]
        public long LoginDateEpoch { get; set; }

        public string LoginDate
        {
            get
            {
                DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(LoginDateEpoch);
                return $"{date.Day}/{date:MMM}/{date.Year}";
            }
        }

        [JsonProperty("hat")]
        public string Hat { get; set; }

        [JsonProperty("head")]
        public string Head { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("feet")]
        public string Feet { get; set; }

        [JsonProperty("hatColor")]
        public string HatColor { get; set; }

        [JsonProperty("headColor")]
        public string HeadColor { get; set; }

        [JsonProperty("bodyColor")]
        public string BodyColor { get; set; }

        [JsonProperty("feetColor")]
        public string FeetColor { get; set; }

        [JsonProperty("hatColor2")]
        public string HatColor2 { get; set; }

        [JsonProperty("headColor2")]
        public string HeadColor2 { get; set; }

        [JsonProperty("bodyColor2")]
        public string BodyColor2 { get; set; }

        [JsonProperty("feetColor2")]
        public string FeetColor2 { get; set; }

        [JsonProperty("exp_points")]
        public ulong ExpPoints { get; set; }

        [JsonProperty("exp_to_rank")]
        public ulong ExpToRank { get; set; }

        [JsonProperty("friend")]
        public int Friend { get; set; }

        [JsonProperty("ignored")]
        public int Ignored { get; set; }      
    }
}