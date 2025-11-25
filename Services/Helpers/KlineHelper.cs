using Repositories.DTOs;
using System.Globalization;
using System.Text.Json;

namespace Services.Helpers
{
    public class KlineHelper
    {
        private readonly HttpClient _httpClient;

        public KlineHelper(HttpClient httpClient)
        {
            _httpClient = httpClient;
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("https://api.binance.com");
            }
        }

        public async Task<List<KlineDTO>> GetKlinesBySymbolAsync(string symbol)
        {
            string url = $"/api/v3/klines?symbol={symbol.ToUpper()}&interval=1h&limit=100";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return new List<KlineDTO>();
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var rawData = JsonSerializer.Deserialize<List<List<object>>>(jsonString);

                var candles = new List<KlineDTO>();

                if (rawData != null)
                {
                    foreach (var item in rawData)
                    {
                        candles.Add(new KlineDTO
                        {
                            OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(item[0].ToString())).UtcDateTime,
                            Open = ParseDecimal(item[1]),
                            High = ParseDecimal(item[2]),
                            Low = ParseDecimal(item[3]),
                            Close = ParseDecimal(item[4]),
                            Volume = ParseDecimal(item[5])
                        });
                    }
                }

                return candles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"KlineHelper Hatası: {ex.Message}");
                return new List<KlineDTO>();
            }
        }

        private decimal ParseDecimal(object value)
        {
            if (value == null) return 0;
            return decimal.Parse(value.ToString(), CultureInfo.InvariantCulture);
        }
    }
}