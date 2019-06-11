namespace DueDex.Internal
{
    internal class ApiKeyPair
    {
        public string ApiKey { get; }
        public string ApiSecret { get; }

        public ApiKeyPair(string apiKey, string apiSecret)
        {
            ApiKey = apiKey;
            ApiSecret = apiSecret;
        }
    }
}