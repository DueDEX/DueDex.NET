# DueDex.NET

The official C# client SDK for DueDEX

## Installation

The quickest way to install the client SDK is by using the [NuGet package](https://www.nuget.org/packages/DueDex/):

    dotnet add package DueDex

## Getting Started

To start using the client, add the following `using` directive to your code file:

    using DueDex;

Then, create a client to be used throughout the application. The client object is thread-safe and you should store it to be reused:

    // Creates an anonymous client to access public APIs ONLY
    var anonymousClient = new DueDexClient();

    // Creates an anonymous client that connects to the public TESTNET
    var anonymousTestnetClient = new DueDexClient(NetworkType.Testnet);

    // Creates an authenticated client
    var authenticatedClient = new DueDexClient("YOUR_API_KEY", "YOUR_API_SECRET");

    // Creates an authenticated client to the public TESTNET
    var authenticatedClient = new DueDexClient("YOUR_API_KEY", "YOUR_API_SECRET", NetworkType.Testnet);

You can also provide an `ILogger<DueDexClient>` instance to the construtor to log messages and errors.

## Using REST APIs

To interact with REST APIs, simply call the awaitable methods in the client class:

    // Creates an authenticated client
    var client = new DueDexClient("YOUR_API_KEY", "YOUR_API_SECRET");

    // Places a new order
    var order = await client.NewLimitOrderAsync("BTCUSD", OrderSide.Long, 8000, 100);

    // Cancels the order
    await client.CancelOrderAsync("BTCUSD", order.OrderId);

## Using WebSocket APIs

To interact with WebSocket streams, the application defines handlers to listen to events:

    // Creates an authenticated client
    var client = new DueDexClient("YOUR_API_KEY", "YOUR_API_SECRET");

    // Defines an event handler
    client.MatchesUpdated += (object sender, MatchesUpdatedEventArgs eventArgs) =>
    {
        foreach (var match in eventArgs.NewMatches)
            Console.WriteLine($"New match in {match.Instrument}: {match.Size} contracts at price {match.Price}");
    };

    // Starts the WebSocket connection
    client.Subscribe(ChannelType.Matches, "BTCUSD");
    client.StartWebSocket();