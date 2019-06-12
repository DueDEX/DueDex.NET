using System.Runtime.Serialization;

namespace DueDex
{
    /// <summary>
    /// Order type
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
    }
}