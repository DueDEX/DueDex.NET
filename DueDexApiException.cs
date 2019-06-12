using System;

namespace DueDex
{
    /// <summary>
    /// The exception that is thrown when a non-success response from DueDEX is received.
    /// </summary>
    public class DueDexApiException : Exception
    {
        /// <summary>
        /// The exception code.
        /// </summary>
        public int Code { get; }

        public DueDexApiException(int code, string message) : base(message)
        {
            Code = code;
        }
    }
}