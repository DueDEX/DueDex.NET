using System;
using System.Collections.Generic;
using DueDex.Models;

namespace DueDex
{
    /// <summary>
    /// Provides data for the DueDexClient.ExecutionsUpdated event.
    /// </summary>
    public class ExecutionsUpdatedEventArgs : EventArgs
    {
        public string Instrument { get; }
        public IEnumerable<Execution> NewExecutions { get; }
        public DateTime Timestamp { get; }

        public ExecutionsUpdatedEventArgs(string instrument, IEnumerable<Execution> newExecutions, DateTime timestamp)
        {
            Instrument = instrument;
            NewExecutions = newExecutions;
            Timestamp = timestamp;
        }
    }
}