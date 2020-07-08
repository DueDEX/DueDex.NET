using System.Runtime.Serialization;

namespace DueDex
{
    /// <summary>
    /// Market status
    /// </summary>
    public enum MarketStatus
    {
        /// <summary>
        /// Created but not yet opened for trading
        /// </summary>
        [EnumMember(Value = "created")]
        Created = 0,
        /// <summary>
        /// Open for trading
        /// </summary>
        [EnumMember(Value = "open")]
        Open = 1,
        /// <summary>
        /// Suspended
        /// </summary>
        [EnumMember(Value = "suspended")]
        Suspended = 2,
        /// <summary>
        /// Delisted
        /// </summary>
        [EnumMember(Value = "delisted")]
        Delisted = 3,
    }
}