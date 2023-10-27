namespace StockService.Messages
{
    public class ReserveStockCancelCommand : ICommand
    {
        public string ReserveId { get; set; }
    }
}