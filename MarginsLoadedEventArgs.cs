using System;
using System.Collections.Generic;
using DueDex.Models;

namespace DueDex
{
    /// <summary>
    /// Provides data for the DueDexClient.MarginsLoaded event.
    /// </summary>
    public class MarginsLoadedEventArgs : EventArgs
    {
        public IReadOnlyDictionary<string, Margin> Margins { get; }
        public DateTime Timestamp { get; }

        public MarginsLoadedEventArgs(IReadOnlyDictionary<string, Margin> margins, DateTime timestamp)
        {
            Margins = margins;
            Timestamp = timestamp;
        }
    }
}