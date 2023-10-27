namespace OrderService.Messages
{
    public class BillingGenerationResponse : IMessage
    {
        public string OrderId { get; set; }
    }
}