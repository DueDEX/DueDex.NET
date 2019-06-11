using System.Runtime.Serialization;

namespace DueDex
{
    public enum OrderType
    {
        [EnumMember(Value = "limit")]
        Limit = 0,
        [EnumMember(Value = "market")]
        Market = 1,
    }
}