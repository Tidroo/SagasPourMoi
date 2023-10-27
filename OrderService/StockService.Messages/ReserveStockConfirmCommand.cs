namespace StockService.Messages
{
    public class ReserveStockConfirmCommand : ICommand
    {
        public string ReserveId { get; set; }
    }
}