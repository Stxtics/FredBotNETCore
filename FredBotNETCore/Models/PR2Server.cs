using Newtonsoft.Json;
using System;

namespace FredBotNETCore.Models
{
    public class PR2Server
    {
        [JsonProperty("server_id")]
        public int Id { get; set; }

        [JsonProperty("server_name")]
        public string Name { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("population")]
        public int Population { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("guild_id")]
        public int GuildId { get; set; }

        [JsonProperty("tournament")]
        public int Tournament { get; set; }

        [JsonProperty("happy_hour")]
        public int HappyHour { get; set; }

        public override string ToString()
        {
            if (Status.Equals("down", StringComparison.InvariantCultureIgnoreCase) && Id < 12)
            {
                return Name + " (down)";
            }
            else if (HappyHour == 1 && Id < 12)
            {
                return "!! " + Name + " (" + Population + " online)";
            }
            else if (HappyHour == 1 && Id > 11)
            {
                return "* !! " + Name + " (" + Population + " online)";
            }
            else if (Status.Equals("down", StringComparison.InvariantCultureIgnoreCase) && Id > 10)
            {
                return "* " + Name + " (down)";
            }
            else if (Id > 11)
            {
                return "* " + Name + " (" + Population + " online)";
            }
            else
            {
                return Name + " (" + Population + " online)";
            }
        }
    }
}