namespace DueDex.Internal
{
    internal class PositionUpdate
    {
        public string Instrument { get; set; }
        public long? Quantity { get; set; }
        public decimal? Leverage { get; set; }
        public decimal? EntryValue { get; set; }
        public decimal? EntryPrice { get; set; }
        public decimal? MarkPrice { get; set; }
        public decimal? LiquidationPrice { get; set; }
        public decimal? PositionMargin { get; set; }
        public decimal? BuyOrderMargin { get; set; }
        public decimal? SellOrderMargin { get; set; }
        public decimal? UnrealisedPnl { get; set; }
        public decimal? RealisedPnl { get; set; }
        public decimal? RiskValue { get; set; }
        public decimal? RiskLimit { get; set; }

        public PositionUpdate(string instrument, long? quantity, decimal? leverage, decimal? entryValue, decimal? entryPrice, decimal? markPrice, decimal? liquidationPrice, decimal? positionMargin, decimal? buyOrderMargin, decimal? sellOrderMargin, decimal? unrealisedPnl, decimal? realisedPnl, decimal? riskValue, decimal? riskLimit)
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
    }
}