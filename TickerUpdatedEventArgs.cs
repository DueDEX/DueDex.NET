using System;
using System.Collections.Generic;
using DueDex.Models;

namespace DueDex
{
    /// <summary>
    /// Provides data for the DueDexClient.TickerUpdated event.
    /// </summary>
    public class TickerUpdatedEventArgs : EventArgs
    {
        public string Instrument { get; }
        public Ticker Ticker { get; }
        public DateTime Timestamp { get; }

        public TickerUpdatedEventArgs(string instrument, Ticker ticker, DateTime timestamp)
        {
            Instrument = instrument;
            Ticker = ticker;
            Timestamp = timestamp;
        }
    }
}