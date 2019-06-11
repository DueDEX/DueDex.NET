using Newtonsoft.Json;

namespace DueDex.Internal
{
    internal class AuthRequest
    {
        [JsonProperty]
        public const string Type = "auth";
        [JsonProperty]
        public string Key { get; set; }
        [JsonProperty]
        public string Answer { get; set; }

        public AuthRequest(string key, string answer)
        {
            Key = key;
            Answer = answer;
        }
    }
}