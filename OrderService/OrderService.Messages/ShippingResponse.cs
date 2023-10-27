namespace OrderService.Messages
{
    public class ShippingResponse : IMessage
    {
        public string OrderId { get; set; }
    }
}