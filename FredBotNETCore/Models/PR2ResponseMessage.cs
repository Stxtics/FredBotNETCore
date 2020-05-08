using Newtonsoft.Json;

namespace FredBotNETCore.Models
{
    public class PR2ResponseMessage
    {
        public PR2ResponseMessage(bool success, string message = null, string error = null)
        {
            Success = success;
            Message = message;
            Error = error;
        }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}