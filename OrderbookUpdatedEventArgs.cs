using System;
using DueDex.Models;

namespace DueDex
{
    public class OrderbookUpdatedEventArgs : EventArgs
    {
        public string Instrument { get; }
        public Orderbook Orderbook { get; }
        public DateTime Timestamp { get; }

        public OrderbookUpdatedEventArgs(string instrument, Orderbook orderbook, DateTime timestamp)
        {
            Instrument = instrument;
            Orderbook = orderbook;
            Timestamp = timestamp;
        }
    }
}