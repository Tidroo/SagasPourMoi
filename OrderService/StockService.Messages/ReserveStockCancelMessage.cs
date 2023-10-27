namespace StockService.Messages
{
    /// <summary>
    /// Message send to the Originator of the ReserveStockCommand if the ReserveStockCancelCommand was made from another party
    /// </summary>
    public class ReserveStockCancelMessage : IMessage
    {
        public string? ReserveId { get; set; }
        public string? OrderId { get; set; }
    }
}