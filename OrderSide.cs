using System.Runtime.Serialization;

namespace DueDex
{
    /// <summary>
    /// Represents an order's direction
    /// </summary>
    public enum OrderSide
    {
        /// <summary>
        /// Buy / long order
        /// </summary>
        [EnumMember(Value = "long")]
        Long = 0,
        /// <summary>
        /// Sell / short order
        /// </summary>
        [EnumMember(Value = "short")]
        Short = 1,
    }
}