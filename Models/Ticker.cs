using System;
using System.Threading;

namespace DueDex.Models
{
    public class Ticker
    {
        /// <summary>
        /// Id of the instrument
        /// </summary>
        public string Instrument { get; internal set; }
        /// <summary>
        /// The real-time best bid price
        /// </summary>
        public decimal BestBid { get; internal set; }
        /// <summary>
        /// The real-time best ask price
        /// </summary>
        public decimal BestAsk { get; internal set; }
        /// <summary>
        /// The real-time last price
        /// </summary>
        public decimal LastPrice { get; internal set; }
        /// <summary>
        /// The real-time index price
        /// </summary>
        public decimal IndexPrice { get; internal set; }
        /// <summary>
        /// The real-time mark price
        /// </summary>
        public decimal MarkPrice { get; internal set; }
        /// <summary>
        /// The funding rate of the current period
        /// </summary>
        public decimal FundingRate { get; internal set; }
        /// <summary>
        /// Timestamp of the end of the current funding period
        /// </summary>
        public DateTime NextFundingTime { get; internal set; }
        /// <summary>
        /// The open price of the sliding 24-hour window. Updated every 5 seconds
        /// </summary>
        public decimal Open { get; internal set; }
        /// <summary>
        /// The high price of the sliding 24-hour window. Updated every 5 seconds
        /// </summary>
        public decimal High { get; internal set; }
        /// <summary>
        /// The low price of the sliding 24-hour windo. Updated every 5 seconds
        /// </summary>
        public decimal Low { get; internal set; }
        /// <summary>
        /// The close price of the sliding 24-hour window. Updated every 5 seconds
        /// </summary>
        public decimal Close { get; internal set; }
        /// <summary>
        /// The number of contracts traded in the sliding 24-hour window. Updated every 5 seconds
        /// </summary>
        public long Volume { get; internal set; }
        /// <summary>
        /// The real-time open interest in contracts
        /// </summary>
        public long OpenInterest { get; internal set; }
        /// <summary>
        /// The US-dollar-dominated value of contracts traded in the sliding 24-hour window. Updated every 5 seconds
        /// </summary>
        public decimal VolumeUsd { get; internal set; }

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public Ticker(string instrument, decimal bestBid, decimal bestAsk, decimal lastPrice, decimal indexPrice, decimal markPrice, decimal fundingRate, DateTime nextFundingTime, decimal open, decimal high, decimal low, decimal close, long volume, long openInterest, decimal volumeUsd)
        {
            Instrument = instrument;
            BestBid = bestBid;
            BestAsk = bestAsk;
            LastPrice = lastPrice;
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
            VolumeUsd = volumeUsd;
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