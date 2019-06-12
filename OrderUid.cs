using System.Collections.Generic;

namespace DueDex
{
    /// <summary>
    /// A globally unique identifier for orders.
    /// </summary>
    public class OrderUid
    {
        /// <summary>
        /// Id of the instrument the order is under
        /// </summary>
        public string Instrument { get; }
        /// <summary>
        /// The order id assigned by the exchange
        /// </summary>
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