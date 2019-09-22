using System.Runtime.Serialization;

namespace DueDex
{
    /// <summary>
    /// Stop trigger type
    /// </summary>
    public enum StopTriggerType
    {
        /// <summary>
        /// Use the last traded price on DueDEX as trigger
        /// </summary>
        [EnumMember(Value = "lastPrice")]
        LastPrice = 0,
        /// <summary>
        /// Use the index price as trigger
        /// </summary>
        [EnumMember(Value = "indexPrice")]
        IndexPrice = 1,
        /// <summary>
        /// Use the mark price as trigger
        /// </summary>
        [EnumMember(Value = "markPrice")]
        MarkPrice = 2,
    }
}