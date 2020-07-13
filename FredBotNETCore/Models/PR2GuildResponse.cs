using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

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
        public List<PR2GuildMember> Members
        {
            get
            {
                PR2GuildMember member = _members.Where(x => x.Id == Guild.OwnerId).FirstOrDefault();
                if (member != null)
                {
                    member.IsOwner = true;
                }
                return _members;
            }
            set
            {
                _members = value;
            }
        }
        private List<PR2GuildMember> _members;
    }
}