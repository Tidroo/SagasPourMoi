namespace OrderService.Saga
{
    public class MarkOrderSagaAsComplete : IMessage
    {
        public string OrderId { get; set; }
    }
}
