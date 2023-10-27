namespace OrderService.Messages
{
    public class PaymentCompletionResponse : IMessage
    {
        public string OrderId { get; set; }
        public bool Success { get; set; }
    }
}