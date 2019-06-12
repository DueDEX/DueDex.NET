using System.Runtime.Serialization;

namespace DueDex
{
    /// <summary>
    /// Represents an order's time in force
    /// </summary>
    public enum TimeInForce
    {
        /// <summary>
        /// Good till cancelled
        /// </summary>
        [EnumMember(Value = "gtc")]
        Gtc = 0,

        /*
         * GTT orders not yet supported
         *
        /// <summary>
        /// Good till time
        /// </summary>
        [EnumMember(Value = "gtt")]
        Gtt = 1,
        */

        /// <summary>
        /// Immediate or cancel
        /// </summary>
        [EnumMember(Value = "ioc")]
        Ioc = 2,
        /// <summary>
        /// Fill or kill
        /// </summary>
        [EnumMember(Value = "fok")]
        Fok = 3,
    }
}