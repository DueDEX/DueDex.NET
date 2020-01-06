using System.Runtime.Serialization;

namespace DueDex
{
    /// <summary>
    /// Represents an order's type.
    /// </summary>
    public enum OrderType
    {
        /// <summary>
        /// Limit order
        /// </summary>
        [EnumMember(Value = "limit")]
        Limit = 0,
        /// <summary>
        /// Market order
        /// </summary>
        [EnumMember(Value = "market")]
        Market = 1,
        /// <summary>
        /// Stop market order
        /// </summary>
        [EnumMember(Value = "stopMarket")]
        StopMarket = 2,
        /// <summary>
        /// Stop limit order
        /// </summary>
        [EnumMember(Value = "stopLimit")]
        StopLimit = 3,
        /// <summary>
        /// Take profit market order
        /// </summary>
        [EnumMember(Value = "takeProfitMarket")]
        TakeProfitMarket = 4,
        /// <summary>
        /// Take profit limit order
        /// </summary>
        [EnumMember(Value = "takeProfitLimit")]
        TakeProfitLimit = 5,
    }
}