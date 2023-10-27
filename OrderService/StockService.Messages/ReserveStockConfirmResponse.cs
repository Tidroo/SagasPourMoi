namespace StockService.Messages
{
    public class ReserveStockConfirmResponse : IMessage
    {
        public string ReserveId { get; set; }
        public bool Success { get; set; }
        public string? OrderId { get; set; }
    }
}