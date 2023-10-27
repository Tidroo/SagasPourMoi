
using StockService.Messages;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StockService.Handlers
{
    public class StockIncreasedHandler : IHandleMessages<StockIncreaseCommand>
    {
        public Task Handle(StockIncreaseCommand message, IMessageHandlerContext context)
        {
            Console.WriteLine($"StockIncreaseCommand {message.ProductId}");
            return Task.CompletedTask;
        }
    }
}
