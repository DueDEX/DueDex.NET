using System.Runtime.Serialization;

namespace DueDex
{
    /// <summary>
    /// Tick direction
    /// </summary>
    public enum TickDirection
    {
        /// <summary>
        /// Price lower than the last match
        /// </summary>
        MinusTick = 0,
        /// <summary>
        /// Price equal to the last match, but lower than the last match at a different price
        /// </summary>
        ZeroMinusTick = 1,
        /// <summary>
        /// Price equal to the last match, but higher than the last match at a different price
        /// </summary>
        ZeroPlusTick = 2,
        /// <summary>
        /// Price higher than the last match
        /// </summary>
        PlusTick = 3,
    }
}