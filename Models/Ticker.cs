using System;
using System.Threading;

namespace DueDex.Models
{
    public class Ticker
    {
        public string Instrument { get; internal set; }
        public decimal BestBid { get; internal set; }
        public decimal BestAsk { get; internal set; }
        public decimal IndexPrice { get; internal set; }
        public decimal MarkPrice { get; internal set; }
        public decimal FundingRate { get; internal set; }
        public DateTime NextFundingTime { get; internal set; }
        public decimal Open { get; internal set; }
        public decimal High { get; internal set; }
        public decimal Low { get; internal set; }
        public decimal Close { get; internal set; }
        public long Volume { get; internal set; }
        public long OpenInterest { get; internal set; }

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public Ticker(string instrument, decimal bestBid, decimal bestAsk, decimal indexPrice, decimal markPrice, decimal fundingRate, DateTime nextFundingTime, decimal open, decimal high, decimal low, decimal close, long volume, long openInterest)
        {
            Instrument = instrument;
            BestBid = bestBid;
            BestAsk = bestAsk;
            IndexPrice = indexPrice;
            MarkPrice = markPrice;
            FundingRate = fundingRate;
            NextFundingTime = nextFundingTime;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
            OpenInterest = openInterest;
        }

        internal void Convert(Action<Ticker> converter)
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