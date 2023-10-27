namespace OrderService.Saga
{
    public class OrderSagaData : IContainSagaData
    {
        public Guid Id { get; set; }
        public string? Originator { get; set; }
        public string? OriginalMessageId { get; set; }
        public string? OrderId { get; set; }
        public bool PaymentTimedOut { get; set; }
        public bool MarkedForCompletion { get; set; }
        public string ReserveId { get; set; }
        public bool ReserveCanceled { get; set; }
        public bool PaymentSucceed { get; set; }
        public bool PaymentFailed { get; set; }
    }
}
