using System.Text.Json.Serialization;

namespace Repositories.DTOs
{
    public class KlineDTO
    {
        public DateTime OpenTime { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
    }

    public class SocketKlineEvent
    {
        [JsonPropertyName("e")]
        public string EventType { get; set; }

        [JsonPropertyName("E")]
        public long EventTime { get; set; }

        [JsonPropertyName("s")]
        public string Symbol { get; set; }

        [JsonPropertyName("k")]
        public KlineDetail Kline { get; set; }
    }

    // k : {...}
    public class KlineDetail
    {
        [JsonPropertyName("t")]
        public long OpenTime { get; set; }

        [JsonPropertyName("o")]
        public string OpenPrice { get; set; }

        [JsonPropertyName("c")]
        public string ClosePrice { get; set; }

        [JsonPropertyName("h")]
        public string HighPrice { get; set; }

        [JsonPropertyName("l")]
        public string LowPrice { get; set; }

        [JsonPropertyName("v")]
        public string Volume { get; set; }

        [JsonPropertyName("x")]
        public bool IsClosed { get; set; }
    }
}
