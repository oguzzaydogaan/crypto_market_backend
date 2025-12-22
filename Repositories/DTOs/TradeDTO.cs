namespace Repositories.DTOs
{
    public class TradeDTO
    {
        public decimal Price { get; set; }
        public decimal Qty { get; set; }
        public long Time { get; set; }
        public bool IsBuyerMaker { get; set; }
    }
}
