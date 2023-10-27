namespace StockService.Sagas
{
    public class MarkReserveSagaAsComplete : IMessage
    {
        public string ReserveId { get; set; }
    }
}
