namespace StockService.Messages
{
    public class ReserveStockResponse : IMessage
    {
        public string OrderId { get; set; }

        public bool Success { get; set; }
        public string ReserveId { get; set; }
    }
}