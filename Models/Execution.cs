using System;
using System.Threading;

namespace DueDex.Models
{
    public class Execution
    {
        /// <summary>
        /// Id of the instrument
        /// </summary>
        public string Instrument { get; internal set; }
        /// <summary>
        /// Id of the execution. Unique under every instrument
        /// </summary>
        public long ExecutionId { get; internal set; }
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
        public OrderType OrderType { get; internal set; }
        /// <summary>
        /// Order side
        /// </summary>
        public OrderSide OrderSide { get; internal set; }
        /// <summary>
        /// Order price. Sent only when orderType is limit or stopLimit
        /// </summary>
        public decimal? OrderPrice { get; internal set; }
        /// <summary>
        /// The order quantity
        /// </summary>
        public long OrderSize { get; internal set; }
        /// <summary>
        /// Price of execution
        /// </summary>
        public decimal ExecutionPrice { get; internal set; }
        /// <summary>
        /// Quantity of execution
        /// </summary>
        public long ExecutionSize { get; internal set; }
        /// <summary>
        /// Whether the user is maker. true if the order is resting
        /// </summary>
        public bool IsMaker { get; internal set; }
        /// <summary>
        /// Trading fee rate. Negative for rebate
        /// </summary>
        public decimal FeeRate { get; internal set; }
        /// <summary>
        /// Trading fee form the execution. Negative for rebate
        /// </summary>
        public decimal Fee { get; internal set; }
        /// <summary>
        /// Quantity left after execution
        /// </summary>
        public long OrderSizeLeft { get; internal set; }
        /// <summary>
        /// Timestamp of the trade
        /// </summary>
        public DateTime Timestamp { get; internal set; }

        public Execution(string instrument, long executionId, long orderId, string clientOrderId, OrderType orderType, OrderSide orderSide, decimal? orderPrice, long orderSize, decimal executionPrice, long executionSize, bool isMaker, decimal feeRate, decimal fee, long orderSizeLeft, DateTime timestamp)
        {
            Instrument = instrument;
            ExecutionId = executionId;
            OrderId = orderId;
            ClientOrderId = clientOrderId;
            OrderType = orderType;
            OrderSide = orderSide;
            OrderPrice = orderPrice;
            OrderSize = orderSize;
            ExecutionPrice = executionPrice;
            ExecutionSize = executionSize;
            IsMaker = isMaker;
            FeeRate = feeRate;
            Fee = fee;
            OrderSizeLeft = orderSizeLeft;
            Timestamp = timestamp;
        }

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        internal void Convert(Action<Execution> converter)
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
