namespace StockService.Messages
{
    public class ReserveStockCancelResponse : IMessage
    {
        public string ReserveId { get; set; }
        public string? OrderId { get; set; }
    }
}