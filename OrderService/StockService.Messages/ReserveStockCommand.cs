namespace StockService.Messages
{
    public class ReserveStockCommand : ICommand
    {
        public string OrderId { get; set; }
        public string ReserveId { get; set; }

        public Dictionary<string, int> ProductsQuantity { get; set; }
    }
}