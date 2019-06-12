using System;
using System.Collections.Generic;
using DueDex.Models;

namespace DueDex
{
    /// <summary>
    /// Provides data for the DueDexClient.MarginsUpdated event.
    /// </summary>
    public class MarginsUpdatedEventArgs : EventArgs
    {
        public IReadOnlyDictionary<string, Margin> UpdatedMargins { get; }
        public IReadOnlyDictionary<string, Margin> Margins { get; }
        public DateTime Timestamp { get; }

        public MarginsUpdatedEventArgs(IReadOnlyDictionary<string, Margin> updatedMargins, IReadOnlyDictionary<string, Margin> margins, DateTime timestamp)
        {
            UpdatedMargins = updatedMargins;
            Margins = margins;
            Timestamp = timestamp;
        }
    }
}