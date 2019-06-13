using System;

namespace DueDex.Models
{
    public class Match
    {
        /// <summary>
        /// Id of the instrument
        /// </summary>
        public string Instrument { get; }
        /// <summary>
        /// Id of the match. Unique under every instrument
        /// </summary>
        public long MatchId { get; }
        /// <summary>
        /// The match price
        /// </summary>
        public decimal Price { get; }
        /// <summary>
        /// The number of contracts traded
        /// </summary>
        public long Size { get; }
        /// <summary>
        /// The order side of the aggressing order
        /// </summary>
        public OrderSide Side { get; }
        /// <summary>
        /// The tick direction
        /// </summary>
        public TickDirection TickDirection { get; }
        /// <summary>
        /// Timestamp of the trade
        /// </summary>
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