using System;

namespace DueDex.Internal
{
    internal class TickerUpdate
    {
        public string Instrument { get; set; }
        public decimal? BestBid { get; set; }
        public decimal? BestAsk { get; set; }
        public decimal? IndexPrice { get; set; }
        public decimal? MarkPrice { get; set; }
        public decimal? FundingRate { get; set; }
        public DateTime? NextFundingTime { get; set; }
        public decimal? Open { get; set; }
        public decimal? High { get; set; }
        public decimal? Low { get; set; }
        public decimal? Close { get; set; }
        public long? Volume { get; set; }
        public long? OpenInterest { get; set; }

        public TickerUpdate(string instrument, decimal? bestBid, decimal? bestAsk, decimal? indexPrice, decimal? markPrice, decimal? fundingRate, DateTime? nextFundingTime, decimal? open, decimal? high, decimal? low, decimal? close, long? volume, long? openInterest)
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
    }
}
