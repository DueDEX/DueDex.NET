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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using DueDex.Internal;
using DueDex.Models;

namespace DueDex
{
    public class DueDexClient
    {
        public event EventHandler<OrderbookUpdatedEventArgs> OrderbookUpdated;
        public event EventHandler<OrdersUpdatedEventArgs> OrdersUpdated;

        private readonly ApiKeyPair apiKeyPair;
        private readonly string restBaseUrl;
        private readonly string webSocketEndpoint;

        private ClientWebSocket webSocketClient;
        private HttpClient httpClient = new HttpClient();

        private Dictionary<string, Orderbook> orderbooks = new Dictionary<string, Orderbook>();
        private Dictionary<OrderUid, Order> orders;

        private readonly HashSet<Channel> channels = new HashSet<Channel>();

        private readonly JsonSerializer serializer = new JsonSerializer
        {
            Converters = {
                new StringEnumConverter(new CamelCaseNamingStrategy())
            },
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public DueDexClient() : this(NetworkType.Mainnet) { }

        public DueDexClient(NetworkType network)
        {
            this.restBaseUrl = network.GetRestBaseUrl();
            this.webSocketEndpoint = network.GetWebSocketEndpoint();
        }

        public DueDexClient(string apiKey, string apiSecret) : this(apiKey, apiSecret, NetworkType.Mainnet) { }

        public DueDexClient(string apiKey, string apiSecret, NetworkType network) : this(network)
        {
            this.apiKeyPair = new ApiKeyPair(apiKey, apiSecret);
        }

        public DueDexClient(string apiKey, string apiSecret, string restBaseUrl, string webSocketEndpoint)
        {
            this.apiKeyPair = new ApiKeyPair(apiKey, apiSecret);
            this.restBaseUrl = restBaseUrl;
            this.webSocketEndpoint = webSocketEndpoint;
        }

        public async Task<Order> NewLimitCloseOrderAsync(string instrument, decimal price, TimeInForce timeInForce = TimeInForce.Gtc)
        {
            return await NewLimitCloseOrderAsync(instrument, null, price, timeInForce);
        }

        public async Task<Order> NewLimitCloseOrderAsync(string instrument, string clientOrderId, decimal price, TimeInForce timeInForce = TimeInForce.Gtc)
        {
            return await NewOrderAsync(instrument, clientOrderId, OrderType.Limit, true, null, price, null, timeInForce);
        }

        public async Task<Order> NewMarketCloseOrderAsync(string instrument, TimeInForce timeInForce = TimeInForce.Ioc)
        {
            return await NewMarketCloseOrderAsync(instrument, null, timeInForce);
        }

        public async Task<Order> NewMarketCloseOrderAsync(string instrument, string clientOrderId, TimeInForce timeInForce = TimeInForce.Ioc)
        {
            return await NewOrderAsync(instrument, clientOrderId, OrderType.Market, true, null, null, null, timeInForce);
        }

        public async Task<Order> NewLimitOrderAsync(string instrument, OrderSide side, decimal price, long size, TimeInForce timeInForce = TimeInForce.Gtc)
        {
            return await NewLimitOrderAsync(instrument, null, side, price, size, timeInForce);
        }

        public async Task<Order> NewLimitOrderAsync(string instrument, string clientOrderId, OrderSide side, decimal price, long size, TimeInForce timeInForce = TimeInForce.Gtc)
        {
            return await NewOrderAsync(instrument, clientOrderId, OrderType.Limit, false, side, price, size, timeInForce);
        }

        public async Task<Order> NewMarketOrderAsync(string instrument, OrderSide side, long size, TimeInForce timeInForce = TimeInForce.Ioc)
        {
            return await NewMarketOrderAsync(instrument, null, side, size, timeInForce);
        }

        public async Task<Order> NewMarketOrderAsync(string instrument, string clientOrderId, OrderSide side, long size, TimeInForce timeInForce = TimeInForce.Ioc)
        {
            return await NewOrderAsync(instrument, clientOrderId, OrderType.Market, false, side, null, size, timeInForce);
        }

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

        public void StartWebSocket()
        {
            var webSocketThread = new Thread(RunWebSocket);
            webSocketThread.Start();
        }

        private async Task<Order> NewOrderAsync(string instrument, string clientOrderId, OrderType type, bool isCloseOrder, OrderSide? side, decimal? price, long? size, TimeInForce? timeInForce)
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
                    timeInForce = timeInForce
                }
            );
        }

        private async Task SendRestRequestAsync(HttpMethod method, string path, bool authenticate, object pars = null)
        {
            await SendRestRequestAsync<object>(method, path, authenticate, pars);
        }

        private async Task<T> SendRestRequestAsync<T>(HttpMethod method, string path, bool authenticate, object pars = null)
        {
            // Loads pars into sorted list
            var parList = new SortedDictionary<string, string>();
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
                        else
                        {
                            parValueString = parValue.ToString();
                        }

                        parList.Add(propertyInfo.Name, parValueString);
                    }
                }

            Uri requestUri;

            if (method == HttpMethod.Get && parList.Count > 0)
            {
                // Append all parameters on request uri
                var uriBuilder = new UriBuilder($"{restBaseUrl}{path}");
                uriBuilder.Query = string.Join("&", parList.Select(p => $"{p.Key}={WebUtility.UrlEncode(p.Value)}"));
                requestUri = uriBuilder.Uri;
            }
            else
            {
                // Just build the uri
                requestUri = new Uri($"{restBaseUrl}{path}");
            }

            var hrm = new HttpRequestMessage(method, requestUri);

            hrm.Headers.Add("Connection", "Keep-Alive");

            if (method != HttpMethod.Get && parList.Count > 0)
            {
                // Send par list in body
                using (var textWriter = new StringWriter())
                {
                    serializer.Serialize(textWriter, pars);
                    hrm.Content = new StringContent(textWriter.ToString(), Encoding.UTF8, "application/json");
                }
            }

            if (authenticate)
            {
                // Authenticate the request

                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long expiration = timestamp + 30 * 1000;

                // Computes signature
                string parListString = string.Join("&", parList.Select(p => $"{p.Key}={WebUtility.UrlEncode(p.Value)}"));
                string message = $"{method}|{path}|{timestamp}|{expiration}|{parListString}";

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
                        Console.WriteLine($"Message from DueDEX: {messageReceived}");

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
                                else if (channel == "orders")
                                {
                                    var snapshotMessage = JsonConvert.DeserializeObject<ChannelMessage<IEnumerable<Order>>>(messageReceived);

                                    orders = new Dictionary<OrderUid, Order>();
                                    foreach (var order in snapshotMessage.Data)
                                    {
                                        if (order.Status != OrderStatus.Filled && order.Status != OrderStatus.Cancelled)
                                            orders.Add(new OrderUid(order.Instrument, order.OrderId), order);
                                    }

                                    OrdersUpdated?.Invoke(this, new OrdersUpdatedEventArgs(orders, snapshotMessage.Timestamp));
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
                                else if (channel == "orders")
                                {
                                    var updateMessage = JsonConvert.DeserializeObject<ChannelMessage<IEnumerable<OrderUpdate>>>(messageReceived);

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
                                        }
                                        else
                                        {
                                            // New order
                                            var newOrder = new Order(
                                                update.Instrument,
                                                update.OrderId,
                                                update.Type.Value,
                                                update.IsCloseOrder.Value,
                                                update.Side.Value,
                                                update.Price ?? 0,
                                                update.Size.Value,
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
                                        }
                                    }

                                    OrdersUpdated?.Invoke(this, new OrdersUpdatedEventArgs(orders, updateMessage.Timestamp));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing DueDEX message: {ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in DueDEX client: {ex}");
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
                    new CancellationToken()
                );
            }
        }
    }
}
