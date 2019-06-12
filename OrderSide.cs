using System.Runtime.Serialization;

namespace DueDex
{
    /// <summary>
    /// Order side
    /// </summary>
    public enum OrderSide
    {
        /// <summary>
        /// Buy order
        /// </summary>
        [EnumMember(Value = "long")]
        Long = 0,
        /// <summary>
        /// Sell order
        /// </summary>
        [EnumMember(Value = "short")]
        Short = 1,
    }
}