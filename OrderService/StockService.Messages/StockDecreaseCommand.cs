namespace StockService.Messages
{
    public class StockDecreaseCommand : ICommand
    {
        public string ProductId { get; set; }

        public int Quantity { get; set; }
    }
}