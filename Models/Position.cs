using System;
using System.Threading;

namespace DueDex.Models
{
    public class Position
    {
        /// <summary>
        /// Id of the instrument
        /// </summary>
        public string Instrument { get; internal set; }
        /// <summary>
        /// The number of contracts holding. Negative for short positions
        /// </summary>
        public long Quantity { get; internal set; }
        /// <summary>
        /// The position leverage. 0 for cross margin mode
        /// </summary>
        public decimal Leverage { get; internal set; }
        /// <summary>
        /// The entry value in settlement currency
        /// </summary>
        public decimal EntryValue { get; internal set; }
        /// <summary>
        /// The average entry price of the current position. 0 if no active position
        /// </summary>
        public decimal EntryPrice { get; internal set; }
        /// <summary>
        /// The current mark price. 0 if no active position
        /// </summary>
        public decimal MarkPrice { get; internal set; }
        /// <summary>
        /// The liquidation price. 0 if no active position
        /// </summary>
        public decimal LiquidationPrice { get; internal set; }
        /// <summary>
        /// Amount of maintanance margin allocated to the position
        /// </summary>
        public decimal PositionMargin { get; internal set; }
        /// <summary>
        /// Amount of order margin allocated to open buy orders
        /// </summary>
        public decimal BuyOrderMargin { get; internal set; }
        /// <summary>
        /// Amount of order margin allocated to open sell orders
        /// </summary>
        public decimal SellOrderMargin { get; internal set; }
        /// <summary>
        /// Unrealised pnl in settlement currency
        /// </summary>
        public decimal UnrealisedPnl { get; internal set; }
        /// <summary>
        /// Realised pnl of the current active position
        /// </summary>
        public decimal RealisedPnl { get; internal set; }
        /// <summary>
        /// Risk value of the current active position
        /// </summary>
        public decimal RiskValue { get; internal set; }
        /// <summary>
        /// The current risk limit
        /// </summary>
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