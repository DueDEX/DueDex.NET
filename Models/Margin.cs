using System;
using System.Threading;

namespace DueDex.Models
{
    public class Margin
    {
        public string Currency { get; internal set; }
        public decimal Available { get; internal set; }
        public decimal OrderMargin { get; internal set; }
        public decimal PositionMargin { get; internal set; }
        public decimal RealisedPnl { get; internal set; }
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
