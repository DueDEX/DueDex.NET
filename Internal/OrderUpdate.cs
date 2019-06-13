using System;

namespace DueDex.Internal
{
    internal class OrderUpdate
    {
        public string Instrument { get; set; }
        public long OrderId { get; set; }
        public OrderType? Type { get; set; }
        public bool? IsCloseOrder { get; set; }
        public OrderSide? Side { get; set; }
        public decimal? Price { get; set; }
        public long? Size { get; set; }
        public TimeInForce? TimeInForce { get; set; }
        public decimal? NotionalValue { get; set; }
        public OrderStatus? Status { get; set; }
        public decimal? FillPrice { get; set; }
        public long? FilledSize { get; set; }
        public decimal? AccumulatedFees { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }

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
