using System;
using System.Collections.Generic;
using DueDex.Models;

namespace DueDex
{
    /// <summary>
    /// Provides data for the DueDexClient.PositionsUpdated event.
    /// </summary>
    public class PositionsUpdatedEventArgs : EventArgs
    {
        public IReadOnlyDictionary<string, Position> UpdatedPositions { get; }
        public IReadOnlyDictionary<string, Position> Positions { get; }
        public DateTime Timestamp { get; }

        public PositionsUpdatedEventArgs(IReadOnlyDictionary<string, Position> updatedPositions, IReadOnlyDictionary<string, Position> positions, DateTime timestamp)
        {
            UpdatedPositions = updatedPositions;
            Positions = positions;
            Timestamp = timestamp;
        }
    }
}