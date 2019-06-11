using System;

namespace DueDex.Internal
{
    public class OrderUpdate
    {
        public string Instrument { get; internal set; }
        public long OrderId { get; internal set; }
        public OrderType? Type { get; internal set; }
        public bool? IsCloseOrder { get; internal set; }
        public OrderSide? Side { get; internal set; }
        public decimal? Price { get; internal set; }
        public long? Size { get; internal set; }
        public TimeInForce? TimeInForce { get; internal set; }
        public decimal? NotionalValue { get; internal set; }
        public OrderStatus? Status { get; internal set; }
        public decimal? FillPrice { get; internal set; }
        public long? FilledSize { get; internal set; }
        public decimal? AccumulatedFees { get; internal set; }
        public DateTime? CreateTime { get; internal set; }
        public DateTime? UpdateTime { get; internal set; }

        public OrderUpdate(string instrument, long orderId, OrderType? type, bool? isCloseOrder, OrderSide? side, decimal? price, long? size, TimeInForce? timeInForce, decimal? notionalValue, OrderStatus? status, decimal? fillPrice, long? filledSize, decimal? accumulatedFees, DateTime? createTime, DateTime? updateTime)
        {
            Instrument = instrument;
            OrderId = orderId;
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
    }
}
