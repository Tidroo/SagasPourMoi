namespace OrderService.Messages
{
    public class PlaceOrderCommand : ICommand
    {
        public string OrderId { get; set; }

        public Dictionary<string, int> ProductsQuantity { get; set; }

        // Payment methods, shipping options, ...
    }
}