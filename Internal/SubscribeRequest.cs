using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DueDex.Internal
{
    internal class SubscribeRequest
    {
        [JsonProperty]
        public const string Type = "subscribe";
        [JsonProperty]
        public IEnumerable<SubscribeChannel> Channels { get; set; }

        public SubscribeRequest(SubscribeChannel channel)
        {
            Channels = new SubscribeChannel[] { channel };
        }

        public SubscribeRequest(IEnumerable<SubscribeChannel> channels)
        {
            Channels = channels;
        }
    }
}