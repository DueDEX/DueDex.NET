using System.Runtime.Serialization;

namespace DueDex
{
    public enum OrderSide
    {
        [EnumMember(Value = "long")]
        Long = 0,
        [EnumMember(Value = "short")]
        Short = 1,
    }
}