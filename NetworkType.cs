using System;
using System.ComponentModel;
using DueDex.Internal;

namespace DueDex
{
    /// <summary>
    /// DueDEX network type
    /// </summary>
    public enum NetworkType
    {
        /// <summary>
        /// DueDEX mainnet
        /// </summary>
        Mainnet = 0,
        /// <summary>
        /// DueDEX testnet
        /// </summary>
        Testnet = 1,
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class NetworkTypeExtensions
    {
        public static string GetRestBaseUrl(this NetworkType network)
        {
            switch (network)
            {
                case NetworkType.Mainnet:
                    return NetworkUrls.MainnetRestBaseUrl;
                case NetworkType.Testnet:
                    return NetworkUrls.TestnetRestBaseUrl;
                default:
                    throw new InvalidEnumArgumentException(nameof(network), (int)network, typeof(NetworkType));
            }
        }

        public static string GetWebSocketEndpoint(this NetworkType network)
        {
            switch (network)
            {
                case NetworkType.Mainnet:
                    return NetworkUrls.MainnetWebSocketEndpoint;
                case NetworkType.Testnet:
                default:
                    throw new InvalidEnumArgumentException(nameof(network), (int)network, typeof(NetworkType));
            }
        }
    }
}