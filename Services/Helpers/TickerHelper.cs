using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repositories.Entities;
using Services.Hubs;
using System.Text.Json;
using Websocket.Client;

namespace Services.Helpers
{
    public class TickerHelper : BackgroundService
    {
        private readonly ILogger<TickerHelper> _logger;
        private readonly IHubContext<CryptoHub> _hubContext;
        private readonly IServiceScopeFactory _scopeFactory;
        private WebsocketClient _client;

        public TickerHelper(ILogger<TickerHelper> logger, IHubContext<CryptoHub> hubContext, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _hubContext = hubContext;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<Coin> coins;
            using (var scope = _scopeFactory.CreateScope())
            {
                var coinService = scope.ServiceProvider.GetRequiredService<CoinService>();
                coins = await coinService.GetAllCoinsAsync();
            }
            var streams = coins.Select(c => "/" + c.Symbol.ToLower() + "@ticker");
            var streamQuery = string.Join("", streams);
            var url = new Uri("wss://stream.binance.com:9443/ws" + streamQuery);

            _client = new WebsocketClient(url);
            _client.ReconnectTimeout = TimeSpan.FromSeconds(5);
            _client.MessageReceived.Subscribe(async msg =>
            {
                if (msg.Text != null)
                {
                    var data = JsonSerializer.Deserialize<TickerData>(msg.Text);
                    await _hubContext.Clients.All.SendAsync("ReceiveTicker", data);
                }
            });

            await _client.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
