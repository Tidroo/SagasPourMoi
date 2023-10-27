using StockService.Messages;

namespace StockService.Handlers
{
    public class StockDecreasedHandler : IHandleMessages<StockDecreaseCommand>
    {
        public Task Handle(StockDecreaseCommand message, IMessageHandlerContext context)
        {
            Console.WriteLine($"StockDecreaseCommand {message.ProductId}");
            return Task.CompletedTask;
        }
    }
}
