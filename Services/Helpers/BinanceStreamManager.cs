using Microsoft.AspNetCore.SignalR;
using Services.Hubs;
using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using Websocket.Client;

namespace Services.Helpers
{
    public class BinanceStreamManager
    {
        private readonly IHubContext<CryptoHub> _hubContext;
        private readonly ConcurrentDictionary<string, WebsocketClient> _activeSockets = new();
        private readonly ConcurrentDictionary<string, int> _subscriberCounts = new();

        public BinanceStreamManager(IHubContext<CryptoHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task JoinStreamAsync(string symbol)
        {
            symbol = symbol.ToLower();
            _subscriberCounts.AddOrUpdate(symbol, 1, (key, count) => count + 1);

            if (_activeSockets.ContainsKey(symbol)) return;

            await StartBinanceSocket(symbol);
        }
        public async Task LeaveStreamAsync(string symbol)
        {
            symbol = symbol.ToLower();

            if (_subscriberCounts.TryGetValue(symbol, out int count))
            {
                int newCount = count - 1;
                if (newCount <= 0)
                {
                    _subscriberCounts.TryRemove(symbol, out _);
                    StopBinanceSocket(symbol);
                }
                else
                {
                    _subscriberCounts.TryUpdate(symbol, newCount, count);
                }
            }
        }
        private async Task StartBinanceSocket(string symbol)
        {
            var url = new Uri($"wss://stream.binance.com:9443/stream?streams={symbol}@kline_1h/{symbol}@trade");

            var client = new WebsocketClient(url);
            client.ReconnectTimeout = TimeSpan.FromSeconds(5);

            client.MessageReceived.Subscribe(async msg =>
            {
                if (msg.Text != null)
                {
                    try
                    {
                        var jsonNode = JsonNode.Parse(msg.Text);
                        var streamName = jsonNode?["stream"]?.ToString();
                        var data = jsonNode?["data"];

                        if (data == null || streamName == null) return;
                        if (streamName.EndsWith("@kline_1h"))
                        {
                            var k = data["k"];
                            if (k != null)
                            {
                                var cleanData = new
                                {
                                    s = data["s"]?.ToString(),              // Symbol
                                    t = k["t"]?.GetValue<long>(),           // Time
                                    o = k["o"]?.ToString(),                 // Open
                                    c = k["c"]?.ToString(),                 // Close
                                    h = k["h"]?.ToString(),                 // High
                                    l = k["l"]?.ToString(),                 // Low
                                    v = k["v"]?.ToString(),                 // Volume
                                    x = k["x"]?.GetValue<bool>()            // IsClosed
                                };
                                await _hubContext.Clients.Group(symbol.ToUpper()).SendAsync("ReceiveKline", cleanData);
                            }
                        }
                        else if (streamName.EndsWith("@trade"))
                        {
                            var cleanTrade = new
                            {
                                s = data["s"]?.ToString(),        // Symbol
                                p = data["p"]?.ToString(),        // Price
                                q = data["q"]?.ToString(),        // Quantity
                                T = data["T"]?.GetValue<long>(),  // Trade Time
                                m = data["m"]?.GetValue<bool>()   // IsBuyerMaker (True=Sell, False=Buy)
                            };
                            await _hubContext.Clients.Group(symbol.ToUpper()).SendAsync("ReceiveTrade", cleanTrade);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SOCKET PARSE ERROR] {symbol}: {ex.Message}");
                    }
                }
            });

            await client.Start();
            _activeSockets.TryAdd(symbol, client);
            Console.WriteLine($"[MANAGER] {symbol.ToUpper()} için Kline & Trade yayını başladı.");
        }
        private void StopBinanceSocket(string symbol)
        {
            if (_activeSockets.TryRemove(symbol, out var client))
            {
                client.Dispose();
                Console.WriteLine($"[MANAGER] {symbol.ToUpper()} yayını DURDURULDU.");
            }
        }
    }
}