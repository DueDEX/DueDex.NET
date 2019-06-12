using System;
using System.Collections.Generic;
using DueDex.Models;

namespace DueDex
{
    /// <summary>
    /// Provides data for the DueDexClient.OrdersUpdated event.
    /// </summary>
    public class OrdersUpdatedEventArgs : EventArgs
    {
        public IReadOnlyDictionary<OrderUid, Order> UpdatedOrders { get; }
        public IReadOnlyDictionary<OrderUid, Order> ActiveOrders { get; }
        public DateTime Timestamp { get; }

        public OrdersUpdatedEventArgs(IReadOnlyDictionary<OrderUid, Order> updatedOrders, IReadOnlyDictionary<OrderUid, Order> activeOrders, DateTime timestamp)
        {
            UpdatedOrders = updatedOrders;
            ActiveOrders = activeOrders;
            Timestamp = timestamp;
        }
    }
}