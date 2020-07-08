using System.Runtime.Serialization;

namespace DueDex
{
    /// <summary>
    /// Contract expiration type
    /// </summary>
    public enum ExpirationType
    {
        /// <summary>
        /// Perpetual. Contract does not expire.
        /// </summary>
        [EnumMember(Value = "perpetual")]
        Perpetual = 0,
    }
}