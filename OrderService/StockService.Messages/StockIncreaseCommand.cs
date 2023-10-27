namespace StockService.Messages
{
    public class StockIncreaseCommand : ICommand
    {
        public string ProductId { get; set; }

        public int Quantity { get; set; }
    }
}