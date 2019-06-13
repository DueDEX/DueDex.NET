using System;
using System.Collections.Generic;
using DueDex.Models;

namespace DueDex
{
    /// <summary>
    /// Provides data for the DueDexClient.PositionsLoaded event.
    /// </summary>
    public class PositionsLoadedEventArgs : EventArgs
    {
        public IReadOnlyDictionary<string, Position> Positions { get; }
        public DateTime Timestamp { get; }

        public PositionsLoadedEventArgs(IReadOnlyDictionary<string, Position> positions, DateTime timestamp)
        {
            Positions = positions;
            Timestamp = timestamp;
        }
    }
}