using System.Collections.Generic;

namespace DueDex.Internal
{
    internal class Channel
    {
        public ChannelType Name { get; }
        public string Instrument { get; }

        public Channel(ChannelType name)
        {
            Name = name;
        }

        public Channel(ChannelType name, string instrument) : this(name)
        {
            Instrument = instrument;
        }

        public override bool Equals(object obj)
        {
            return obj is Channel channel &&
                   Name == channel.Name &&
                   Instrument == channel.Instrument;
        }

        public override int GetHashCode()
        {
            var hashCode = -1484601530;
            hashCode = hashCode * -1521134295 + Name.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Instrument);
            return hashCode;
        }
    }
}