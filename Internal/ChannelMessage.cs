using System;

namespace DueDex.Internal
{
    internal class ChannelMessage<T>
    {
        public string Type { get; set; }
        public string Channel { get; set; }
        public string Instrument { get; set; }
        public T Data { get; set; }
        public DateTime Timestamp { get; set; }

        public ChannelMessage(string type, string channel, string instrument, T data, DateTime timestamp)
        {
            Type = type;
            Channel = channel;
            Instrument = instrument;
            Data = data;
            Timestamp = timestamp;
        }
    }
}