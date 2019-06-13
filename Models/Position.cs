using System;
using System.Threading;

namespace DueDex.Models
{
    public class Position
    {
        public string Instrument { get; internal set; }
        public long Quantity { get; internal set; }
        public decimal Leverage { get; internal set; }
        public decimal EntryValue { get; internal set; }
        public decimal EntryPrice { get; internal set; }
        public decimal MarkPrice { get; internal set; }
        public decimal LiquidationPrice { get; internal set; }
        public decimal PositionMargin { get; internal set; }
        public decimal BuyOrderMargin { get; internal set; }
        public decimal SellOrderMargin { get; internal set; }
        public decimal UnrealisedPnl { get; internal set; }
        public decimal RealisedPnl { get; internal set; }
        public decimal RiskValue { get; internal set; }
        public decimal RiskLimit { get; internal set; }

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public Position(string instrument, long quantity, decimal leverage, decimal entryValue, decimal entryPrice, decimal markPrice, decimal liquidationPrice, decimal positionMargin, decimal buyOrderMargin, decimal sellOrderMargin, decimal unrealisedPnl, decimal realisedPnl, decimal riskValue, decimal riskLimit)
        {
            Instrument = instrument;
            Quantity = quantity;
            Leverage = leverage;
            EntryValue = entryValue;
            EntryPrice = entryPrice;
            MarkPrice = markPrice;
            LiquidationPrice = liquidationPrice;
            PositionMargin = positionMargin;
            BuyOrderMargin = buyOrderMargin;
            SellOrderMargin = sellOrderMargin;
            UnrealisedPnl = unrealisedPnl;
            RealisedPnl = realisedPnl;
            RiskValue = riskValue;
            RiskLimit = riskLimit;
        }

        internal void Convert(Action<Position> converter)
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