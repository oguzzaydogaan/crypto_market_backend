using System.Text.Json.Serialization;

namespace Repositories.Entities
{
    public class TickerData
    {
        [JsonPropertyName("s")]
        public string Symbol { get; set; }

        [JsonPropertyName("c")]
        public string LastPrice { get; set; }

        [JsonPropertyName("P")]
        public string PriceChangePercent { get; set; }
    }
}