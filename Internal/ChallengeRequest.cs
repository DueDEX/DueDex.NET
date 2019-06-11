using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DueDex.Internal
{
    internal class ChallengeRequest
    {
        [JsonProperty]
        public const string Type = "challenge";
    }
}