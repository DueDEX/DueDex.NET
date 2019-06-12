namespace DueDex
{
    /// <summary>
    /// DueDEX WebSocket channel type
    /// </summary>
    public enum ChannelType
    {
        /// <summary>
        /// The level2 channel
        /// </summary>
        Level2 = 1,
        /// <summary>
        /// The ticker channel
        /// </summary>
        Ticker = 2,
        /// <summary>
        /// The matches channel
        /// </summary>
        Matches = 3,
        /// <summary>
        /// The orders channel
        /// </summary>
        Orders = 4,
        /// <summary>
        /// The positions channel
        /// </summary>
        Positions = 5,
        /// <summary>
        /// The margins channel
        /// </summary>
        Margins = 6,
    }
}