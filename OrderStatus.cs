namespace DueDex
{
    /// <summary>
    /// Order status
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// The active order has no fills so far.
        /// </summary>
        New = 0,
        /// <summary>
        /// The active order is partially filled.
        /// </summary>
        PartiallyFilled = 1,
        /// <summary>
        /// The order has been completely filled.
        /// </summary>
        Filled = 2,
        /// <summary>
        /// The order is cancelled, with or without fills.
        /// </summary>
        Cancelled = 3,
    }
}