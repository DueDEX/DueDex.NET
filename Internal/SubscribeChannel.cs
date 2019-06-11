using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DueDex.Internal
{
    internal class SubscribeChannel
    {
        public ChannelType Name { get; }
        public string[] Instruments { get; }

        public SubscribeChannel(ChannelType name)
        {
            Name = name;
        }

        public SubscribeChannel(ChannelType name, string[] instruments) : this(name)
        {
            Instruments = instruments;
        }
    }
}