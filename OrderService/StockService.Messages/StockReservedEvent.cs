namespace StockService.Messages
{
    public class StockReservedEvent : IEvent
    {
        public string ReserveId { get; set; }

        public Dictionary<string, int> ProductsQuantity { get; set; }
    }
}