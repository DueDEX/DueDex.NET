using System.Runtime.Serialization;

namespace DueDex
{
    /// <summary>
    /// Method used for calculating mark price
    /// </summary>
    public enum MarkMethod
    {
        /// <summary>
        /// Last price marking
        /// </summary>
        [EnumMember(Value = "lastPrice")]
        LastPrice = 0,
        /// <summary>
        /// Fair price marking
        /// </summary>
        [EnumMember(Value = "fairPrice")]
        FairPrice = 1,
    }
}