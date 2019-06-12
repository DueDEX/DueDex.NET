using System;

namespace DueDex.Models
{
    public class Match
    {
        public string Instrument { get; }
        public long MatchId { get; }
        public decimal Price { get; }
        public long Size { get; }
        public OrderSide Side { get; }
        public TickDirection TickDirection { get; }
        public DateTime Timestamp { get; }

        public Match(string instrument, long matchId, decimal price, long size, OrderSide side, TickDirection tickDirection, DateTime timestamp)
        {
            Instrument = instrument;
            MatchId = matchId;
            Price = price;
            Size = size;
            Side = side;
            TickDirection = tickDirection;
            Timestamp = timestamp;
        }
    }
}