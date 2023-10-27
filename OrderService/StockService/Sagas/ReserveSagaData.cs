namespace StockService.Sagas
{
    public class ReserveSagaData : IContainSagaData
    {
        public Guid Id { get; set; }
        public string? Originator { get; set; }
        public string? OriginalMessageId { get; set; }
        public Dictionary<string, int>? ProductsQuantity { get; set; }

        public string? ReserveId { get; set; }
        public string? OrderId { get; set; }

        public bool ReserveTimedOut { get; set; }
        public bool MarkedForCompletion { get; set; }
        /*
         * TODO Design as a state machine instead
         * Reserved | Confirmed | Canceled
         */
        public bool ReserveConfirmed { get; set; }
        public bool ReserveCanceled { get; set; }
    }
}
