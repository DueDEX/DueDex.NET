using System;
using System.Collections.Generic;
using DueDex.Models;

namespace DueDex
{
    /// <summary>
    /// Provides data for the DueDexClient.MatchesUpdated event.
    /// </summary>
    public class MatchesUpdatedEventArgs : EventArgs
    {
        public string Instrument { get; }
        public IEnumerable<Match> NewMatches { get; }
        public DateTime Timestamp { get; }

        public MatchesUpdatedEventArgs(string instrument, IEnumerable<Match> newMatches, DateTime timestamp)
        {
            Instrument = instrument;
            NewMatches = newMatches;
            Timestamp = timestamp;
        }
    }
}