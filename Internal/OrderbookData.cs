namespace DueDex.Internal
{
    internal class OrderbookData
    {
        public decimal[][] Bids { get; set; }
        public decimal[][] Asks { get; set; }

        public OrderbookData(decimal[][] bids, decimal[][] asks)
        {
            Bids = bids;
            Asks = asks;
        }
    }
}