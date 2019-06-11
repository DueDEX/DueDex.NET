using System;

namespace DueDex
{
    public class DueDexApiException : Exception
    {
        public int Code { get; }

        public DueDexApiException(int code, string message) : base(message)
        {
            Code = code;
        }
    }
}