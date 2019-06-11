using System.Collections.Generic;

namespace DueDex
{
    public class OrderUid
    {
        public string Instrument { get; }
        public long OrderId { get; }

        public OrderUid(string instrument, long orderId)
        {
            Instrument = instrument;
            OrderId = orderId;
        }

        public override bool Equals(object obj)
        {
            return obj is OrderUid uid &&
                   Instrument == uid.Instrument &&
                   OrderId == uid.OrderId;
        }

        public override int GetHashCode()
        {
            var hashCode = -1237063830;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Instrument);
            hashCode = hashCode * -1521134295 + OrderId.GetHashCode();
            return hashCode;
        }
    }
}