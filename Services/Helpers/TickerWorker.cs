using Microsoft.Extensions.Hosting;

namespace Services.Helpers
{
    public class TickerWorker : BackgroundService
    {
        private readonly BinanceTickerManager _tickerManager;

        public TickerWorker(BinanceTickerManager tickerManager)
        {
            _tickerManager = tickerManager;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _tickerManager.StartAsync();

            // Background service'in kapanmaması için sonsuz döngüde bekletiyoruz
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}