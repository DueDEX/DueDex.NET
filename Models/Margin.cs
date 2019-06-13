using System;
using System.Threading;

namespace DueDex.Models
{
    public class Margin
    {
        /// <summary>
        /// Currency symbol
        /// </summary>
        public string Currency { get; internal set; }
        /// <summary>
        /// The available balance amount of the currency
        /// </summary>
        public decimal Available { get; internal set; }
        /// <summary>
        /// The amount allocated to open orders
        /// </summary>
        public decimal OrderMargin { get; internal set; }
        /// <summary>
        /// The amount allocated as position maintainance margin
        /// </summary>
        public decimal PositionMargin { get; internal set; }
        /// <summary>
        /// The total realised pnl in this currency
        /// </summary>
        public decimal RealisedPnl { get; internal set; }
        /// <summary>
        /// The unrealised pnl of open positions calculated at mark prices
        /// </summary>
        public decimal UnrealisedPnl { get; internal set; }

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public Margin(string currency, decimal available, decimal orderMargin, decimal positionMargin, decimal realisedPnl, decimal unrealisedPnl)
        {
            Currency = currency;
            Available = available;
            OrderMargin = orderMargin;
            PositionMargin = positionMargin;
            RealisedPnl = realisedPnl;
            UnrealisedPnl = unrealisedPnl;
        }

        internal void Convert(Action<Margin> converter)
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
