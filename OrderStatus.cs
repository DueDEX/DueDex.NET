namespace DueDex
{
    /// <summary>
    /// Order status
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// New open order without any fills
        /// </summary>
        New = 0,
        /// <summary>
        /// Open order partillay filled
        /// </summary>
        PartiallyFilled = 1,
        /// <summary>
        /// Order completely filled
        /// </summary>
        Filled = 2,
        /// <summary>
        /// Order cancelled
        /// </summary>
        Cancelled = 3,
    }
}