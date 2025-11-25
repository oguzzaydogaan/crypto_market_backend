using Microsoft.AspNetCore.SignalR;
using Repositories.DTOs;
using Services.Hubs;
using System.Collections.Concurrent;
using System.Text.Json;
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
            var url = new Uri($"wss://stream.binance.com:9443/ws/{symbol.ToLower()}@kline_1h");

            var client = new WebsocketClient(url);
            client.ReconnectTimeout = TimeSpan.FromSeconds(5);

            client.MessageReceived.Subscribe(async msg =>
            {
                if (msg.Text != null)
                {
                    try
                    {
                        var rawData = JsonSerializer.Deserialize<SocketKlineEvent>(msg.Text);
                        if (rawData != null && rawData.Kline != null)
                        {
                            var cleanData = new
                            {
                                s = rawData.Symbol,           // Sembol (BNBBTC)
                                t = rawData.Kline.OpenTime,   // Zaman
                                o = rawData.Kline.OpenPrice,  // Açılış
                                c = rawData.Kline.ClosePrice, // Kapanış (Şu anki fiyat)
                                h = rawData.Kline.HighPrice,  // Yüksek
                                l = rawData.Kline.LowPrice,   // Düşük
                                v = rawData.Kline.Volume,     // Hacim
                                x = rawData.Kline.IsClosed    // Mum bitti mi?
                            };
                            await _hubContext.Clients.Group(symbol.ToUpper()).SendAsync("ReceiveKline", cleanData);
                            Console.WriteLine($"--> {symbol}: {cleanData.c}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SOCKET HATA] Parse edilemedi: {ex.Message}");
                    }
                }
            });

            await client.Start();
            _activeSockets.TryAdd(symbol, client);
            Console.WriteLine($"[MANAGER] {symbol.ToUpper()} için dinleme başladı.");
        }

        private void StopBinanceSocket(string symbol)
        {
            if (_activeSockets.TryRemove(symbol, out var client))
            {
                client.Dispose();
                Console.WriteLine($"[MANAGER] {symbol.ToUpper()} yayını DURDURULDU (İzleyen kalmadı).");
            }
        }
    }
}
