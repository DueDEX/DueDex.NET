using System;

namespace DueDex.Internal
{
    internal class ChallengeMessage
    {
        public string Type { get; set; }
        public Guid Challenge { get; set; }
    }
}