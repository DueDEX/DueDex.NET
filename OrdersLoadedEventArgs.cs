using System;
using System.Collections.Generic;
using DueDex.Models;

namespace DueDex
{
    public class OrdersLoadedEventArgs : EventArgs
    {
        public IReadOnlyDictionary<OrderUid, Order> ActiveOrders { get; }
        public DateTime Timestamp { get; }

        public OrdersLoadedEventArgs(IReadOnlyDictionary<OrderUid, Order> activeOrders, DateTime timestamp)
        {
            ActiveOrders = activeOrders;
            Timestamp = timestamp;
        }
    }
}