using System;
using System.Threading;

namespace DueDex.Models
{
    public class Order
    {
        /// <summary>
        /// Id of the instrument
        /// </summary>
        public string Instrument { get; internal set; }
        /// <summary>
        /// Order id
        /// </summary>
        public long OrderId { get; internal set; }
        /// <summary>
        /// Client order id
        /// </summary>
        public string ClientOrderId { get; internal set; }
        /// <summary>
        /// Order type
        /// </summary>
        public OrderType Type { get; internal set; }
        /// <summary>
        /// Whether the order is a close order
        /// </summary>
        public bool IsCloseOrder { get; internal set; }
        /// <summary>
        /// Order side
        /// </summary>
        public OrderSide Side { get; internal set; }
        /// <summary>
        /// Order price. Sent only when type is limit
        /// </summary>
        public decimal Price { get; internal set; }
        /// <summary>
        /// Order size
        /// </summary>
        public long Size { get; internal set; }
        /// <summary>
        /// Time in force of the order
        /// </summary>
        public TimeInForce TimeInForce { get; internal set; }
        /// <summary>
        /// The order's notional value in settlement currency
        /// </summary>
        public decimal NotionalValue { get; internal set; }
        /// <summary>
        /// Order status
        /// </summary>
        public OrderStatus Status { get; internal set; }
        /// <summary>
        /// The average fill price. 0 if no matches
        /// </summary>
        public decimal FillPrice { get; internal set; }
        /// <summary>
        /// The size matched
        /// </summary>
        public long FilledSize { get; internal set; }
        /// <summary>
        /// The accumulated fees incurred by this order
        /// </summary>
        public decimal AccumulatedFees { get; internal set; }
        /// <summary>
        /// The timestamp of order placement
        /// </summary>
        public DateTime CreateTime { get; internal set; }
        /// <summary>
        /// The timestamp of last update
        /// </summary>
        public DateTime UpdateTime { get; internal set; }

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public Order(string instrument, long orderId, string clientOrderId, OrderType type, bool isCloseOrder, OrderSide side, decimal price, long size, TimeInForce timeInForce, decimal notionalValue, OrderStatus status, decimal fillPrice, long filledSize, decimal accumulatedFees, DateTime createTime, DateTime updateTime)
        {
            Instrument = instrument;
            OrderId = orderId;
            ClientOrderId = clientOrderId;
            Type = type;
            IsCloseOrder = isCloseOrder;
            Side = side;
            Price = price;
            Size = size;
            TimeInForce = timeInForce;
            NotionalValue = notionalValue;
            Status = status;
            FillPrice = fillPrice;
            FilledSize = filledSize;
            AccumulatedFees = accumulatedFees;
            CreateTime = createTime;
            UpdateTime = updateTime;
        }

        internal void Convert(Action<Order> converter)
        {
            try
            {
                _lock.EnterWriteLock();

                converter(this);
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
        }
    }
}
