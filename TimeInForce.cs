using System.Runtime.Serialization;

namespace DueDex
{
    public enum TimeInForce
    {
        [EnumMember(Value = "gtc")]
        Gtc = 0,
        [EnumMember(Value = "gtt")]
        Gtt = 1,
        [EnumMember(Value = "ioc")]
        Ioc = 2,
        [EnumMember(Value = "fok")]
        Fok = 3,
    }
}