using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Repositories.Entities;
using Services.Hubs;
using System.Text.Json;
using Websocket.Client;

namespace Services.Helpers
{
    public class BinanceTickerManager
    {
        private readonly ILogger<BinanceTickerManager> _logger;
        private readonly IHubContext<CryptoHub> _hubContext;
        private readonly IServiceScopeFactory _scopeFactory;
        private WebsocketClient _client;
        private bool _isConnected = false;

        public BinanceTickerManager(ILogger<BinanceTickerManager> logger, IHubContext<CryptoHub> hubContext, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _hubContext = hubContext;
            _scopeFactory = scopeFactory;
        }
        public async Task StartAsync()
        {
            var url = new Uri("wss://stream.binance.com:9443/ws");
            _client = new WebsocketClient(url);
            _client.ReconnectTimeout = TimeSpan.FromSeconds(5);

            _client.ReconnectionHappened.Subscribe(info =>
            {
                _logger.LogInformation($"Reconnection happened, type: {info.Type}");
                _isConnected = true;
                _ = SubscribeAllCoinsFromDb();
            });

            _client.MessageReceived.Subscribe(async msg =>
            {
                if (msg.Text != null)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveTicker", JsonDocument.Parse(msg.Text));
                }
            });

            await _client.Start();
            _isConnected = true;
            await SubscribeAllCoinsFromDb();
        }
        public void SubscribeToCoin(string symbol)
        {
            if (!_isConnected || _client == null) return;

            var request = new
            {
                method = "SUBSCRIBE",
                @params = new[] { $"{symbol.ToLower()}@ticker" },
                id = DateTime.Now.Ticks
            };

            var json = JsonSerializer.Serialize(request);
            _client.Send(json);
            _logger.LogInformation($"Subscribed to new coin: {symbol}");
        }
        private async Task SubscribeAllCoinsFromDb()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var coinService = scope.ServiceProvider.GetRequiredService<CoinService>();
                var coins = await coinService.GetAllCoinsAsync();

                if (coins.Any())
                {
                    var streams = coins.Select(c => $"{c.Symbol.ToLower()}@ticker").ToArray();
                    var request = new
                    {
                        method = "SUBSCRIBE",
                        @params = streams,
                        id = 1
                    };

                    await Task.Delay(1000);
                    _client.Send(JsonSerializer.Serialize(request));
                    _logger.LogInformation($"Subscribed to {coins.Count} coins from DB.");
                }
            }
        }
    }
}