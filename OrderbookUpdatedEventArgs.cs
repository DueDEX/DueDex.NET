using System;
using DueDex.Models;

namespace DueDex
{
    /// <summary>
    /// Provides data for the DueDexClient.OrderbookUpdated event.
    /// </summary>
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