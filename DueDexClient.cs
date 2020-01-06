using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using DueDex.Internal;
using DueDex.Models;

namespace DueDex
{
    /// <summary>
    /// The client for interacting with DueDEX REST and WebSocket APIs.
    /// </summary>
    public class DueDexClient
    {
        /// <summary>
        /// Occurs when an orderbook is initialized or updated.
        /// </summary>
        public event EventHandler<OrderbookUpdatedEventArgs> OrderbookUpdated;
        /// <summary>
        /// Occurs when new matches happen under an instrument.
        /// </summary>
        public event EventHandler<MatchesUpdatedEventArgs> MatchesUpdated;
        /// <summary>
        /// Occurs when instrument tickers are updated.
        /// </summary>
        public event EventHandler<TickerUpdatedEventArgs> TickerUpdated;
        /// <summary>
        /// Occurs when the list of active orders is loaded.
        /// </summary>
        public event EventHandler<OrdersLoadedEventArgs> OrdersLoaded;
        /// <summary>
        /// Occurs when orders are updated.
        /// </summary>
        public event EventHandler<OrdersUpdatedEventArgs> OrdersUpdated;
        /// <summary>
        /// Occurs when the list of margins is loaded.
        /// </summary>
        public event EventHandler<MarginsLoadedEventArgs> MarginsLoaded;
        /// <summary>
        /// Occurs when margins are updated.
        /// </summary>
        public event EventHandler<MarginsUpdatedEventArgs> MarginsUpdated;
        /// <summary>
        /// Occurs when the list of positions is loaded.
        /// </summary>
        public event EventHandler<PositionsLoadedEventArgs> PositionsLoaded;
        /// <summary>
        /// Occurs when positions are updated.
        /// </summary>
        public event EventHandler<PositionsUpdatedEventArgs> PositionsUpdated;
        /// <summary>
        /// Occurs when executions are updated.
        /// </summary>
        public event EventHandler<ExecutionsUpdatedEventArgs> ExecutionsUpdated;

        private readonly ILogger logger;

        private readonly ApiKeyPair apiKeyPair;
        private readonly string restBaseUrl;
        private readonly string webSocketEndpoint;

        private Thread webSocketThread;

        private ClientWebSocket webSocketClient;
        private HttpClient httpClient = new HttpClient();

        private Dictionary<string, Orderbook> orderbooks = new Dictionary<string, Orderbook>();
        private Dictionary<string, Ticker> tickers = new Dictionary<string, Ticker>();
        private Dictionary<OrderUid, Order> orders;
        private Dictionary<string, Margin> margins;
        private Dictionary<string, Position> positions;

        private readonly HashSet<Channel> channels = new HashSet<Channel>();

        private readonly JsonSerializer serializer = new JsonSerializer
        {
            Converters = {
                new StringEnumConverter(new CamelCaseNamingStrategy())
            },
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// Initialize an anonymous client to DueDEX mainnet.
        /// </summary>
        /// <param name="logger">Logger</param>
        public DueDexClient(ILogger<DueDexClient> logger = null) : this(NetworkType.Mainnet, logger) { }

        /// <summary>
        /// Initialize an anonymous client to the specified network.
        /// </summary>
        /// <param name="network">The DueDEX network to connect to</param>
        /// <param name="logger">Logger</param>
        public DueDexClient(NetworkType network, ILogger<DueDexClient> logger = null) : this(null, null, network, logger) { }

        /// <summary>
        /// Initialize an authenticated client DueDEX mainnet.
        /// </summary>
        /// <param name="apiKey">DueDEX API key</param>
        /// <param name="apiSecret">DueDEX API secret</param>
        /// <param name="logger">Logger</param>
        public DueDexClient(string apiKey, string apiSecret, ILogger<DueDexClient> logger = null) : this(apiKey, apiSecret, NetworkType.Mainnet, logger) { }

        /// <summary>
        /// Initialize an authenticated client to the specified network.
        /// </summary>
        /// <param name="apiKey">DueDEX API key</param>
        /// <param name="apiSecret">DueDEX API secret</param>
        /// <param name="network">The DueDEX network to connect to</param>
        /// <param name="logger">Logger</param>
        public DueDexClient(string apiKey, string apiSecret, NetworkType network, ILogger<DueDexClient> logger = null) : this(apiKey, apiSecret, network.GetRestBaseUrl(), network.GetWebSocketEndpoint(), logger) { }

        private DueDexClient(string apiKey, string apiSecret, string restBaseUrl, string webSocketEndpoint, ILogger<DueDexClient> logger = null)
        {
            if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiSecret))
                this.apiKeyPair = new ApiKeyPair(apiKey, apiSecret);
            this.restBaseUrl = restBaseUrl;
            this.webSocketEndpoint = webSocketEndpoint;
            this.logger = logger;
        }

        public Task<Ticker> GetTickerAsync(string instrument)
        {
            return SendRestRequestAsync<Ticker>(
                HttpMethod.Get,
                $"/v1/ticker/{instrument}",
                false
            );
        }

        public Task<Order> GetOrderAsync(string instrument, long orderId)
        {
            return SendRestRequestAsync<Order>(
                HttpMethod.Get,
                $"/v1/order",
                true,
                new
                {
                    instrument = instrument,
                    orderId = orderId
                }
            );
        }

        public Task<Order> GetOrderAsync(string instrument, string clientOrderId)
        {
            return SendRestRequestAsync<Order>(
                HttpMethod.Get,
                $"/v1/order",
                true,
                new
                {
                    instrument = instrument,
                    clientOrderId = clientOrderId
                }
            );
        }

        public Task<Margin> GetMarginAsync(string currencySymbol)
        {
            return SendRestRequestAsync<Margin>(
                HttpMethod.Get,
                $"/v1/margin/{currencySymbol}",
                true
            );
        }

        public Task<Position> GetPositionAsync(string instrument)
        {
            return SendRestRequestAsync<Position>(
                HttpMethod.Get,
                $"/v1/position/{instrument}",
                true
            );
        }

        public async Task<IList<Order>> GetActiveOrdersAsync()
        {
            return await SendRestRequestAsync<Order[]>(
                HttpMethod.Get,
                $"/v1/order/active",
                true
            );
        }

        /// <summary>
        /// Place a new limit order.
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="side">Order side</param>
        /// <param name="price">The limit order price</param>
        /// <param name="size">Order size</param>
        /// <param name="timeInForce">Order time in force</param>
        /// <param name="postOnly">Whether this order can only be maker</param>
        /// <param name="isCloseOrder">Whether this order can only reduce position</param>
        /// <returns>Info of the new order</returns>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public Task<Order> NewLimitOrderAsync(string instrument, OrderSide side, decimal price, long size, TimeInForce timeInForce = TimeInForce.Gtc, bool postOnly = false, bool isCloseOrder = false)
        {
            return NewLimitOrderAsync(instrument, null, side, price, size, timeInForce, postOnly, isCloseOrder);
        }

        /// <summary>
        /// Place a new limit order with a custom client order id.
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="clientOrderId">The custom client order id with a maximum length of 36 characters</param>
        /// <param name="side">Order side</param>
        /// <param name="price">The limit order price</param>
        /// <param name="size">Order size</param>
        /// <param name="timeInForce">Order time in force</param>
        /// <param name="postOnly">Whether this order can only be maker</param>
        /// <param name="isCloseOrder">Whether this order can only reduce position</param>
        /// <returns>Info of the new order</returns>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public Task<Order> NewLimitOrderAsync(string instrument, string clientOrderId, OrderSide side, decimal price, long size, TimeInForce timeInForce = TimeInForce.Gtc, bool postOnly = false, bool isCloseOrder = false)
        {
            return NewOrderAsync(instrument, clientOrderId, OrderType.Limit, isCloseOrder, side, price, size, null, null, timeInForce, postOnly);
        }

        /// <summary>
        /// Place a new market order.
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="side">Order side</param>
        /// <param name="size">Order size</param>
        /// <param name="isCloseOrder">Whether this order can only reduce position</param>
        /// <returns>Info of the new order</returns>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public Task<Order> NewMarketOrderAsync(string instrument, OrderSide side, long size, bool isCloseOrder = false)
        {
            return NewMarketOrderAsync(instrument, null, side, size, isCloseOrder);
        }

        /// <summary>
        /// Place a new market order with a custom client order id.
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="clientOrderId">The custom client order id with a maximum length of 36 characters</param>
        /// <param name="side">Order side</param>
        /// <param name="size">Order size</param>
        /// <param name="isCloseOrder">Whether this order can only reduce position</param>
        /// <returns>Info of the new order</returns>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public Task<Order> NewMarketOrderAsync(string instrument, string clientOrderId, OrderSide side, long size, bool isCloseOrder = false)
        {
            return NewOrderAsync(instrument, clientOrderId, OrderType.Market, false, side, null, size, null, null, TimeInForce.Ioc, false);
        }

        /// <summary>
        /// Place a new stop market order.
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="side">Order side</param>
        /// <param name="size">Order size</param>
        /// <param name="stopPrice">Order stop price</param>
        /// <param name="triggerType">Stop trigger type</param>
        /// <param name="isCloseOrder">Whether this order can only reduce position. Defaults to true</param>
        /// <returns>Info of the new order</returns>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public Task<Order> NewStopMarketOrderAsync(string instrument, OrderSide side, long size, decimal stopPrice, StopTriggerType triggerType = StopTriggerType.LastPrice, bool isCloseOrder = true)
        {
            return NewStopMarketOrderAsync(instrument, null, side, size, stopPrice, triggerType, isCloseOrder);
        }

        /// <summary>
        /// Place a new stop market order with a custom client order id.
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="clientOrderId">The custom client order id with a maximum length of 36 characters</param>
        /// <param name="side">Order side</param>
        /// <param name="size">Order size</param>
        /// <param name="stopPrice">Order stop price</param>
        /// <param name="triggerType">Stop trigger type</param>
        /// <param name="isCloseOrder">Whether this order can only reduce position. Defaults to true</param>
        /// <returns>Info of the new order</returns>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public Task<Order> NewStopMarketOrderAsync(string instrument, string clientOrderId, OrderSide side, long size, decimal stopPrice, StopTriggerType triggerType = StopTriggerType.LastPrice, bool isCloseOrder = true)
        {
            return NewOrderAsync(instrument, clientOrderId, OrderType.StopMarket, isCloseOrder, side, null, size, stopPrice, triggerType, TimeInForce.Ioc, false);
        }

        /// <summary>
        /// Place a new stop limit order.
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="side">Order side</param>
        /// <param name="price">The limit order price</param>
        /// <param name="size">Order size</param>
        /// <param name="stopPrice">Order stop price</param>
        /// <param name="triggerType">Stop trigger type</param>
        /// <param name="timeInForce">Order time in force</param>
        /// <param name="postOnly">Whether this order can only be maker</param>
        /// <param name="isCloseOrder">Whether this order can only reduce position. Defaults to true</param>
        /// <returns>Info of the new order</returns>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public Task<Order> NewStopLimitOrderAsync(string instrument, OrderSide side, decimal price, long size, decimal stopPrice, StopTriggerType triggerType = StopTriggerType.LastPrice, TimeInForce timeInForce = TimeInForce.Gtc, bool postOnly = false, bool isCloseOrder = true)
        {
            return NewStopLimitOrderAsync(instrument, null, side, price, size, stopPrice, triggerType, timeInForce, postOnly, isCloseOrder);
        }

        /// <summary>
        /// Place a new stop limit order with a custom client order id.
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="clientOrderId">The custom client order id with a maximum length of 36 characters</param>
        /// <param name="side">Order side</param>
        /// <param name="price">The limit order price</param>
        /// <param name="size">Order size</param>
        /// <param name="stopPrice">Order stop price</param>
        /// <param name="triggerType">Stop trigger type</param>
        /// <param name="timeInForce">Order time in force</param>
        /// <param name="postOnly">Whether this order can only be maker</param>
        /// <param name="isCloseOrder">Whether this order can only reduce position. Defaults to true</param>
        /// <returns>Info of the new order</returns>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public Task<Order> NewStopLimitOrderAsync(string instrument, string clientOrderId, OrderSide side, decimal price, long size, decimal stopPrice, StopTriggerType triggerType = StopTriggerType.LastPrice, TimeInForce timeInForce = TimeInForce.Gtc, bool postOnly = false, bool isCloseOrder = true)
        {
            return NewOrderAsync(instrument, clientOrderId, OrderType.StopLimit, isCloseOrder, side, price, size, stopPrice, triggerType, timeInForce, postOnly);
        }

        /// <summary>
        /// Cancel an open order by order id
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="orderId">The order id assigned by the exchange</param>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public async Task CancelOrderAsync(string instrument, long orderId)
        {
            await SendRestRequestAsync(
                HttpMethod.Delete,
                "/v1/order",
                true,
                new
                {
                    instrument = instrument,
                    orderId = orderId
                }
            );
        }

        /// <summary>
        /// Place a new take profit market order.
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="side">Order side</param>
        /// <param name="size">Order size</param>
        /// <param name="stopPrice">Order stop price</param>
        /// <param name="triggerType">Stop trigger type</param>
        /// <param name="isCloseOrder">Whether this order can only reduce position. Defaults to true</param>
        /// <returns>Info of the new order</returns>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public Task<Order> NewTakeProfitMarketOrderAsync(string instrument, OrderSide side, long size, decimal stopPrice, StopTriggerType triggerType = StopTriggerType.LastPrice, bool isCloseOrder = true)
        {
            return NewTakeProfitMarketOrderAsync(instrument, null, side, size, stopPrice, triggerType, isCloseOrder);
        }

        /// <summary>
        /// Place a new take profit market order with a custom client order id.
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="clientOrderId">The custom client order id with a maximum length of 36 characters</param>
        /// <param name="side">Order side</param>
        /// <param name="size">Order size</param>
        /// <param name="stopPrice">Order stop price</param>
        /// <param name="triggerType">Stop trigger type</param>
        /// <param name="isCloseOrder">Whether this order can only reduce position. Defaults to true</param>
        /// <returns>Info of the new order</returns>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public Task<Order> NewTakeProfitMarketOrderAsync(string instrument, string clientOrderId, OrderSide side, long size, decimal stopPrice, StopTriggerType triggerType = StopTriggerType.LastPrice, bool isCloseOrder = true)
        {
            return NewOrderAsync(instrument, clientOrderId, OrderType.TakeProfitMarket, isCloseOrder, side, null, size, stopPrice, triggerType, TimeInForce.Ioc, false);
        }

        /// <summary>
        /// Place a new take profit limit order.
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="side">Order side</param>
        /// <param name="price">The limit order price</param>
        /// <param name="size">Order size</param>
        /// <param name="stopPrice">Order stop price</param>
        /// <param name="triggerType">Stop trigger type</param>
        /// <param name="timeInForce">Order time in force</param>
        /// <param name="postOnly">Whether this order can only be maker</param>
        /// <param name="isCloseOrder">Whether this order can only reduce position. Defaults to true</param>
        /// <returns>Info of the new order</returns>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public Task<Order> NewTakeProfitLimitOrderAsync(string instrument, OrderSide side, decimal price, long size, decimal stopPrice, StopTriggerType triggerType = StopTriggerType.LastPrice, TimeInForce timeInForce = TimeInForce.Gtc, bool postOnly = false, bool isCloseOrder = true)
        {
            return NewTakeProfitLimitOrderAsync(instrument, null, side, price, size, stopPrice, triggerType, timeInForce, postOnly, isCloseOrder);
        }

        /// <summary>
        /// Place a new take profit limit order with a custom client order id.
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="clientOrderId">The custom client order id with a maximum length of 36 characters</param>
        /// <param name="side">Order side</param>
        /// <param name="price">The limit order price</param>
        /// <param name="size">Order size</param>
        /// <param name="stopPrice">Order stop price</param>
        /// <param name="triggerType">Stop trigger type</param>
        /// <param name="timeInForce">Order time in force</param>
        /// <param name="postOnly">Whether this order can only be maker</param>
        /// <param name="isCloseOrder">Whether this order can only reduce position. Defaults to true</param>
        /// <returns>Info of the new order</returns>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public Task<Order> NewTakeProfitLimitOrderAsync(string instrument, string clientOrderId, OrderSide side, decimal price, long size, decimal stopPrice, StopTriggerType triggerType = StopTriggerType.LastPrice, TimeInForce timeInForce = TimeInForce.Gtc, bool postOnly = false, bool isCloseOrder = true)
        {
            return NewOrderAsync(instrument, clientOrderId, OrderType.TakeProfitLimit, isCloseOrder, side, price, size, stopPrice, triggerType, timeInForce, postOnly);
        }

        /// <summary>
        /// Cancel an open order by client order id
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="clientOrderId">The custom order id</param>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public async Task CancelOrderAsync(string instrument, string clientOrderId)
        {
            await SendRestRequestAsync(
                HttpMethod.Delete,
                "/v1/order",
                true,
                new
                {
                    instrument = instrument,
                    clientOrderId = clientOrderId
                }
            );
        }

        /// <summary>
        /// Cancel all orders under a specific instrument
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public async Task CancelOrdersAsync(string instrument)
        {
            await SendRestRequestAsync(
                HttpMethod.Delete,
                "/v1/order",
                true,
                new
                {
                    instrument = instrument
                }
            );
        }

        /// <summary>
        /// Amend an order's price by order id
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="orderId">The order id assigned by the exchange</param>
        /// <param name="newPrice">The new order price</param>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public async Task AmendOrderPriceAsync(string instrument, long orderId, decimal newPrice)
        {
            await SendRestRequestAsync(
                new HttpMethod("PATCH"),
                "/v1/order",
                true,
                new
                {
                    instrument = instrument,
                    orderId = orderId,
                    price = newPrice
                }
            );
        }

        /// <summary>
        /// Amend an order's price by client order id
        /// </summary>
        /// <param name="instrument">Id of the instrument</param>
        /// <param name="clientOrderId">The custom order id</param>
        /// <param name="newPrice">The new order price</param>
        /// <exception cref="DueDexApiException">Thrown when the server responds with a non-success code</exception>
        public async Task AmendOrderPriceAsync(string instrument, string clientOrderId, decimal newPrice)
        {
            await SendRestRequestAsync(
                new HttpMethod("PATCH"),
                "/v1/order",
                true,
                new
                {
                    instrument = instrument,
                    clientOrderId = clientOrderId,
                    price = newPrice
                }
            );
        }

        /// <summary>
        /// Subscribe to a WebSocket channel
        /// </summary>
        /// <param name="channel">Channel name</param>
        /// <param name="instruments">A list of instruments to subscribe to for instrument channels only</param>
        public void Subscribe(ChannelType channel, params string[] instruments)
        {
            if (instruments.Length > 0)
            {
                foreach (string instrument in instruments)
                {
                    var newChannel = new Channel(channel, instrument);

                    if (!channels.Add(newChannel))
                        return;

                    if (!(webSocketClient is null) && (webSocketClient.State == WebSocketState.Open))
                        _ = SubscribeAsync(newChannel);
                }
            }
            else
            {
                var newChannel = new Channel(channel);

                if (!channels.Add(newChannel))
                    return;

                if (!(webSocketClient is null) && (webSocketClient.State == WebSocketState.Open))
                    _ = SubscribeAsync(newChannel);
            }
        }

        /// <summary>
        /// Start WebSocket connection
        /// </summary>
        public void StartWebSocket()
        {
            if (!(webSocketThread is null))
                throw new InvalidOperationException("DueDex WebSocket client has already been started");

            webSocketThread = new Thread(RunWebSocket);
            webSocketThread.Start();
        }

        private async Task<Order> NewOrderAsync(string instrument, string clientOrderId, OrderType type, bool isCloseOrder, OrderSide? side, decimal? price, long? size, decimal? stopPrice, StopTriggerType? triggerType, TimeInForce? timeInForce, bool postOnly)
        {
            return await SendRestRequestAsync<Order>(
                HttpMethod.Post,
                "/v1/order",
                true,
                new
                {
                    instrument = instrument,
                    clientOrderId = clientOrderId,
                    type = type,
                    isCloseOrder = isCloseOrder,
                    side = side,
                    price = price,
                    size = size,
                    stopPrice = stopPrice,
                    triggerType = triggerType,
                    timeInForce = timeInForce,
                    postOnly = postOnly
                }
            );
        }

        private async Task SendRestRequestAsync(HttpMethod method, string path, bool authenticate, object pars = null)
        {
            await SendRestRequestAsync<object>(method, path, authenticate, pars);
        }

        private async Task<T> SendRestRequestAsync<T>(HttpMethod method, string path, bool authenticate, object pars = null)
        {
            // For signature
            string queryString = "", bodyString = "";

            Uri requestUri;

            if (method == HttpMethod.Get && pars != null)
            {
                // Loads pars into list
                var parList = new Dictionary<string, string>();
                if (!(pars is null))
                    foreach (var propertyInfo in pars.GetType().GetProperties())
                    {
                        var parValue = propertyInfo.GetValue(pars);
                        if (!(parValue is null))
                        {
                            string parValueString;
                            if (parValue is bool boolValue)
                            {
                                parValueString = boolValue ? "true" : "false";
                            }
                            else if (parValue.GetType().IsEnum)
                            {
                                parValueString = parValue.GetType().GetMember(parValue.ToString())[0].GetCustomAttribute<EnumMemberAttribute>().Value;
                            }
                            else if (parValue is decimal decimalValue)
                            {
                                parValueString = JsonConvert.SerializeObject(parValue);
                            }
                            else
                            {
                                parValueString = parValue.ToString();
                            }

                            parList.Add(propertyInfo.Name, parValueString);
                        }
                    }

                // Append all parameters on request uri
                var uriBuilder = new UriBuilder($"{restBaseUrl}{path}");
                queryString = string.Join("&", parList.Select(p => $"{p.Key}={WebUtility.UrlEncode(p.Value)}"));
                uriBuilder.Query = queryString;
                requestUri = uriBuilder.Uri;
            }
            else
            {
                // Just build the uri
                requestUri = new Uri($"{restBaseUrl}{path}");
            }

            var hrm = new HttpRequestMessage(method, requestUri);

            hrm.Headers.Add("Connection", "Keep-Alive");

            if (method != HttpMethod.Get && pars != null)
            {
                // Send par list in body
                using (var textWriter = new StringWriter())
                {
                    serializer.Serialize(textWriter, pars);
                    bodyString = textWriter.ToString();
                    hrm.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");
                }
            }

            if (authenticate)
            {
                // Authenticate the request

                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long expiration = timestamp + 30 * 1000;

                // Computes signature
                string message = $"{method}|{path}|{timestamp}|{expiration}|{queryString}|{bodyString}";

                StringBuilder signatureHex = new StringBuilder();
                using (var hmacsha256 = new HMACSHA256(Convert.FromBase64String(apiKeyPair.ApiSecret)))
                    foreach (byte b in hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(message)))
                        signatureHex.Append(b.ToString("x2"));

                hrm.Headers.Add("Ddx-Timestamp", timestamp.ToString());
                hrm.Headers.Add("Ddx-Expiration", expiration.ToString());
                hrm.Headers.Add("Ddx-Key", apiKeyPair.ApiKey);
                hrm.Headers.Add("Ddx-Signature", signatureHex.ToString());
            }

            var response = await httpClient.SendAsync(hrm);
            string content = await response.Content.ReadAsStringAsync();

            logger?.LogTrace($"REST response from DueDEX on {method} {path}: {content}");
            Console.WriteLine($"REST response from DueDEX on {method} {path}: {content}");

            var resObj = JsonConvert.DeserializeObject<ApiResponse<T>>(content);

            if (resObj.Code != 0)
                throw new DueDexApiException(resObj.Code, resObj.Message);

            return resObj.Data;
        }

        private async void RunWebSocket()
        {
            // Retry loop
            while (true)
            {
                try
                {
                    webSocketClient = new ClientWebSocket();
                    await webSocketClient.ConnectAsync(new Uri(webSocketEndpoint), new CancellationToken());

                    if (apiKeyPair is null)
                    {
                        // No auth possible. Just subscribe
                        if (channels.Count > 0)
                            await SubscribeAsync(channels.ToArray());
                    }
                    else
                    {
                        // Get challenge
                        await SendAsync(new ChallengeRequest());
                    }

                    byte[] buffer = new byte[1024];

                    // Receive loop
                    while (true)
                    {
                        // Receive a message until it's fully built out
                        var msgBuilder = new StringBuilder();
                        while (true)
                        {
                            var result = await webSocketClient.ReceiveAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), new CancellationToken());
                            msgBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                            if (result.EndOfMessage)
                                break;
                        }

                        // Complete message
                        string messageReceived = msgBuilder.ToString();
                        logger?.LogTrace($"Message from DueDEX: {messageReceived}");

                        // Deserialize message
                        try
                        {
                            var jObject = JsonConvert.DeserializeObject(messageReceived) as JObject;
                            string messageType = jObject.GetValue("type").Value<string>();
                            if (messageType == "challenge")
                            {
                                // Processes challenge message
                                var challengeMessage = JsonConvert.DeserializeObject<ChallengeMessage>(messageReceived);

                                var answerHex = new StringBuilder();
                                using (var hmacsha256 = new HMACSHA256(Convert.FromBase64String(apiKeyPair.ApiSecret)))
                                    foreach (byte b in hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(challengeMessage.Challenge.ToString())))
                                        answerHex.Append(b.ToString("x2"));

                                await SendAsync(new AuthRequest(apiKeyPair.ApiKey, answerHex.ToString()));
                            }
                            else if (messageType == "auth")
                            {
                                if (channels.Count > 0)
                                    await SubscribeAsync(channels.ToArray());
                            }
                            else if (messageType == "snapshot")
                            {
                                string channel = jObject.GetValue("channel").Value<string>();

                                if (channel == "level2")
                                {
                                    var snapshotMessage = JsonConvert.DeserializeObject<ChannelMessage<OrderbookData>>(messageReceived);

                                    var orderbook = new Orderbook();
                                    foreach (var ask in snapshotMessage.Data.Asks)
                                        orderbook.UpdateAsk(ask[0], (long)ask[1]);
                                    foreach (var bid in snapshotMessage.Data.Bids)
                                        orderbook.UpdateBid(bid[0], (long)bid[1]);

                                    orderbooks[snapshotMessage.Instrument] = orderbook;

                                    OrderbookUpdated?.Invoke(this, new OrderbookUpdatedEventArgs(snapshotMessage.Instrument, orderbook, snapshotMessage.Timestamp));
                                }
                                else if (channel == "ticker")
                                {
                                    var snapshotMessage = JsonConvert.DeserializeObject<ChannelMessage<Ticker>>(messageReceived);

                                    tickers[snapshotMessage.Instrument] = snapshotMessage.Data;

                                    TickerUpdated?.Invoke(this, new TickerUpdatedEventArgs(snapshotMessage.Instrument, snapshotMessage.Data, snapshotMessage.Timestamp));
                                }
                                else if (channel == "margins")
                                {
                                    var snapshotMessage = JsonConvert.DeserializeObject<ChannelMessage<IEnumerable<Margin>>>(messageReceived);

                                    margins = new Dictionary<string, Margin>();
                                    foreach (var margin in snapshotMessage.Data)
                                    {
                                        margins.Add(margin.Currency, margin);
                                    }

                                    MarginsLoaded?.Invoke(this, new MarginsLoadedEventArgs(margins, snapshotMessage.Timestamp));
                                }
                                else if (channel == "orders")
                                {
                                    var snapshotMessage = JsonConvert.DeserializeObject<ChannelMessage<IEnumerable<Order>>>(messageReceived);

                                    orders = new Dictionary<OrderUid, Order>();
                                    foreach (var order in snapshotMessage.Data)
                                    {
                                        if (order.Status != OrderStatus.Filled && order.Status != OrderStatus.Cancelled)
                                            orders.Add(new OrderUid(order.Instrument, order.OrderId), order);
                                    }

                                    OrdersLoaded?.Invoke(this, new OrdersLoadedEventArgs(orders, snapshotMessage.Timestamp));
                                }
                                else if (channel == "positions")
                                {
                                    var snapshotMessage = JsonConvert.DeserializeObject<ChannelMessage<IEnumerable<Position>>>(messageReceived);

                                    positions = new Dictionary<string, Position>();
                                    foreach (var position in snapshotMessage.Data)
                                    {
                                        positions.Add(position.Instrument, position);
                                    }

                                    PositionsLoaded?.Invoke(this, new PositionsLoadedEventArgs(positions, snapshotMessage.Timestamp));
                                }
                            }
                            else if (messageType == "update")
                            {
                                string channel = jObject.GetValue("channel").Value<string>();

                                if (channel == "level2")
                                {
                                    var updateMessage = JsonConvert.DeserializeObject<ChannelMessage<OrderbookData>>(messageReceived);

                                    var orderbook = orderbooks[updateMessage.Instrument];
                                    foreach (var ask in updateMessage.Data.Asks)
                                        orderbook.UpdateAsk(ask[0], (long)ask[1]);
                                    foreach (var bid in updateMessage.Data.Bids)
                                        orderbook.UpdateBid(bid[0], (long)bid[1]);

                                    OrderbookUpdated?.Invoke(this, new OrderbookUpdatedEventArgs(updateMessage.Instrument, orderbook, updateMessage.Timestamp));
                                }
                                else if (channel == "matches")
                                {
                                    var updateMessage = JsonConvert.DeserializeObject<ChannelMessage<IEnumerable<Match>>>(messageReceived);

                                    MatchesUpdated?.Invoke(this, new MatchesUpdatedEventArgs(updateMessage.Instrument, updateMessage.Data, updateMessage.Timestamp));
                                }
                                else if (channel == "ticker")
                                {
                                    var updateMessage = JsonConvert.DeserializeObject<ChannelMessage<TickerUpdate>>(messageReceived);

                                    var existingTicker = tickers[updateMessage.Instrument];
                                    existingTicker.Convert(order =>
                                    {
                                        foreach (var property in updateMessage.Data.GetType().GetProperties())
                                            if (!(property.GetValue(updateMessage.Data) is null))
                                                existingTicker.GetType().GetProperty(property.Name).SetValue(existingTicker, property.GetValue(updateMessage.Data));
                                    });

                                    TickerUpdated?.Invoke(this, new TickerUpdatedEventArgs(updateMessage.Instrument, existingTicker, updateMessage.Timestamp));
                                }
                                else if (channel == "margins")
                                {
                                    var updateMessage = JsonConvert.DeserializeObject<ChannelMessage<IEnumerable<MarginUpdate>>>(messageReceived);

                                    var updatedMargins = new Dictionary<string, Margin>();

                                    foreach (var update in updateMessage.Data)
                                    {
                                        if (margins.TryGetValue(update.Currency, out var existingMargin))
                                        {
                                            existingMargin.Convert(order =>
                                            {
                                                foreach (var property in update.GetType().GetProperties())
                                                    if (!(property.GetValue(update) is null))
                                                        existingMargin.GetType().GetProperty(property.Name).SetValue(existingMargin, property.GetValue(update));
                                            });

                                            updatedMargins[update.Currency] = existingMargin;
                                        }
                                        else
                                        {
                                            // New currency
                                            var newMargin = new Margin(
                                                update.Currency,
                                                update.Available.Value,
                                                update.OrderMargin.Value,
                                                update.PositionMargin.Value,
                                                update.RealisedPnl.Value,
                                                update.UnrealisedPnl.Value
                                            );

                                            margins.Add(update.Currency, newMargin);

                                            updatedMargins[update.Currency] = newMargin;
                                        }
                                    }

                                    MarginsUpdated?.Invoke(this, new MarginsUpdatedEventArgs(updatedMargins, margins, updateMessage.Timestamp));
                                }
                                else if (channel == "orders")
                                {
                                    var updateMessage = JsonConvert.DeserializeObject<ChannelMessage<IEnumerable<OrderUpdate>>>(messageReceived);

                                    var updatedOrders = new Dictionary<OrderUid, Order>();

                                    foreach (var update in updateMessage.Data)
                                    {
                                        var orderUid = new OrderUid(update.Instrument, update.OrderId);

                                        if (orders.TryGetValue(orderUid, out var existingOrder))
                                        {
                                            existingOrder.Convert(order =>
                                            {
                                                foreach (var property in update.GetType().GetProperties())
                                                    if (!(property.GetValue(update) is null))
                                                        existingOrder.GetType().GetProperty(property.Name).SetValue(existingOrder, property.GetValue(update));
                                            });

                                            if (existingOrder.Status == OrderStatus.Filled || existingOrder.Status == OrderStatus.Cancelled)
                                                orders.Remove(orderUid);

                                            updatedOrders[orderUid] = existingOrder;
                                        }
                                        else
                                        {
                                            // New order
                                            var newOrder = new Order(
                                                update.Instrument,
                                                update.OrderId,
                                                update.ClientOrderId,
                                                update.Type.Value,
                                                update.IsCloseOrder.Value,
                                                update.Side.Value,
                                                update.Price,
                                                update.Size.Value,
                                                update.StopPrice,
                                                update.TriggerType,
                                                update.TimeInForce.Value,
                                                update.NotionalValue.Value,
                                                update.Status.Value,
                                                update.FillPrice.Value,
                                                update.FilledSize.Value,
                                                update.AccumulatedFees.Value,
                                                update.CreateTime.Value,
                                                update.UpdateTime.Value
                                            );

                                            if (newOrder.Status != OrderStatus.Filled && newOrder.Status != OrderStatus.Cancelled)
                                                orders.Add(orderUid, newOrder);

                                            updatedOrders[orderUid] = newOrder;
                                        }
                                    }

                                    OrdersUpdated?.Invoke(this, new OrdersUpdatedEventArgs(updatedOrders, orders, updateMessage.Timestamp));
                                }
                                else if (channel == "positions")
                                {
                                    var updateMessage = JsonConvert.DeserializeObject<ChannelMessage<IEnumerable<PositionUpdate>>>(messageReceived);

                                    var updatedPositions = new Dictionary<string, Position>();

                                    foreach (var update in updateMessage.Data)
                                    {
                                        if (positions.TryGetValue(update.Instrument, out var existingPosition))
                                        {
                                            existingPosition.Convert(order =>
                                            {
                                                foreach (var property in update.GetType().GetProperties())
                                                    if (!(property.GetValue(update) is null))
                                                        existingPosition.GetType().GetProperty(property.Name).SetValue(existingPosition, property.GetValue(update));
                                            });

                                            updatedPositions[update.Instrument] = existingPosition;
                                        }
                                        else
                                        {
                                            // New instrument
                                            var newPosition = new Position(
                                                update.Instrument,
                                                update.Quantity.Value,
                                                update.Leverage.Value,
                                                update.EntryValue.Value,
                                                update.EntryPrice.Value,
                                                update.MarkPrice.Value,
                                                update.LiquidationPrice.Value,
                                                update.PositionMargin.Value,
                                                update.BuyOrderMargin.Value,
                                                update.SellOrderMargin.Value,
                                                update.UnrealisedPnl.Value,
                                                update.RealisedPnl.Value,
                                                update.RiskValue.Value,
                                                update.RiskLimit.Value
                                            );

                                            positions.Add(update.Instrument, newPosition);

                                            updatedPositions[update.Instrument] = newPosition;
                                        }
                                    }

                                    PositionsUpdated?.Invoke(this, new PositionsUpdatedEventArgs(updatedPositions, positions, updateMessage.Timestamp));
                                }
                                else if (channel == "executions")
                                {
                                    var updateMessage = JsonConvert.DeserializeObject<ChannelMessage<IEnumerable<Execution>>>(messageReceived);

                                    ExecutionsUpdated?.Invoke(this, new ExecutionsUpdatedEventArgs(updateMessage.Instrument, updateMessage.Data, updateMessage.Timestamp));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger?.LogError($"Error parsing DueDEX message: {ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError($"Error in DueDEX client: {ex}");
                }
            }
        }

        private async Task SubscribeAsync(params Channel[] channels)
        {
            await SendAsync(
                new SubscribeRequest(
                    channels.Select(
                        c => c.Instrument is null ? new SubscribeChannel(c.Name) : new SubscribeChannel(c.Name, new string[] { c.Instrument })
                    )
                )
            );
        }

        private async Task SendAsync(object value)
        {
            using (var textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, value);

                byte[] buffer = Encoding.UTF8.GetBytes(textWriter.ToString());

                await webSocketClient.SendAsync(
                    new ArraySegment<byte>(buffer, 0, buffer.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
        }
    }
}
