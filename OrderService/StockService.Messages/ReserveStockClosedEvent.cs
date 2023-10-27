namespace StockService.Messages
{
    public class ReserveStockClosedEvent : IEvent
    {
        public string ReserveId { get; set; }

        public Dictionary<string, int> ProductsQuantity { get; set; }
    }
}