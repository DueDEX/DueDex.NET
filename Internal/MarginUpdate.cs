namespace DueDex.Models
{
    public class MarginUpdate
    {
        public string Currency { get; set; }
        public decimal? Available { get; set; }
        public decimal? OrderMargin { get; set; }
        public decimal? PositionMargin { get; set; }
        public decimal? RealisedPnl { get; set; }
        public decimal? UnrealisedPnl { get; set; }

        public MarginUpdate(string currency, decimal? available, decimal? orderMargin, decimal? positionMargin, decimal? realisedPnl, decimal? unrealisedPnl)
        {
            Currency = currency;
            Available = available;
            OrderMargin = orderMargin;
            PositionMargin = positionMargin;
            RealisedPnl = realisedPnl;
            UnrealisedPnl = unrealisedPnl;
        }
    }
}
