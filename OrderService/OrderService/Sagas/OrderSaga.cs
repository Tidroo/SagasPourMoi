using OrderService.Messages;
using StockService.Messages;

namespace OrderService.Saga
{
    public class OrderSagas : Saga<OrderSagaData>, 
        IAmStartedByMessages<PlaceOrderCommand>,
        IHandleMessages<ReserveStockResponse>,
        IHandleMessages<ReserveStockCancelResponse>,
        IHandleMessages<ReserveStockCancelMessage>,
        IHandleMessages<ReserveStockConfirmResponse>,
        IHandleTimeouts<PaymentTimeout>,
        IHandleMessages<PaymentCompletionResponse>,
        IHandleTimeouts<MarkOrderSagaAsCompleteCleanupTimeout>,
        IHandleMessages<MarkOrderSagaAsComplete>
    {
        public OrderSagas()
        {

        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<OrderSagaData> mapper)
        {
            mapper.MapSaga(saga => saga.OrderId)
                .ToMessage<PlaceOrderCommand>(msg => msg.OrderId)
                .ToMessage<ReserveStockResponse>(s => s.OrderId)
                .ToMessage<PaymentCompletionResponse>(s => s.OrderId)
                .ToMessage<ReserveStockCancelMessage>(s => s.OrderId)
                .ToMessage<ReserveStockCancelResponse>(s => s.OrderId)
                .ToMessage<ReserveStockConfirmResponse>(s => s.OrderId);
                //.ToMessage<ShippingCompletedEvent>(s => s.OrderId)
        }

        public async Task Handle(PlaceOrderCommand message, IMessageHandlerContext context)
        {
            Console.WriteLine($"PlaceOrderCommand {message.OrderId}");

            // TODO Deduplication
            //Data.OrderCreated = true;

            Data.ReserveId = Guid.NewGuid().ToString();
            //Data.ProductsQuantity = message.ProductsQuantity;
            await context.Send("StockService", new ReserveStockCommand() { 
                OrderId = message.OrderId,
                ReserveId = Data.ReserveId,
                ProductsQuantity = message.ProductsQuantity
            });
        }

        public async Task Handle(ReserveStockResponse message, IMessageHandlerContext context)
        {
            Console.WriteLine($"ReserveStockResponse {message.ReserveId}");
            
            if(message.Success)
            {
                // Send payment command
                // context.Send(new PaymentCompletionCommand {})

                // Request a timeout if the payment is not confirmed by the banking system in a timely manner
                await RequestTimeout(context, TimeSpan.FromMinutes(2), new PaymentTimeout() { });
            }
            else
            {
                // Do compensation logic, mail users, ...
                throw new NotImplementedException();
            }
        }

        public Task Handle(ReserveStockCancelMessage message, IMessageHandlerContext context)
        {
            Console.WriteLine($"ReserveStockCancelMessage {message.ReserveId}");
            // Do compensation logic
            Data.ReserveCanceled = true;
            return Task.CompletedTask;
        }

        public async Task Timeout(PaymentTimeout state, IMessageHandlerContext context)
        {
            // Send shipping command
            Console.WriteLine($"PaymentTimeout {Data.OrderId}");
            if (Data.PaymentSucceed || Data.PaymentFailed)
                return;

            Data.PaymentTimedOut = true;
            // Do compensation logic
            await context.Send("StockService", new ReserveStockCancelCommand()
            {
                ReserveId = Data.ReserveId
            });

            await this.MarkAsCompleteDelayed(context);
        }

        public async Task Handle(PaymentCompletionResponse message, IMessageHandlerContext context)
        {
            Console.WriteLine($"PaymentCompletedEvent {message.OrderId}");

            if(message.Success)
            {
                Data.PaymentSucceed = true;
                if(Data.PaymentTimedOut)
                {
                    // Do compensation logic, refill payment, mail user, ...

                    await MarkAsCompleteDelayed(context);
                }
                else
                {
                    await context.Send("StockService", new ReserveStockConfirmCommand()
                    {
                        ReserveId = Data.ReserveId
                    });
                }
            }
            else
            {
                Data.PaymentFailed = true;
                await context.Send("StockService", new ReserveStockCancelCommand()
                {
                    ReserveId = Data.ReserveId
                });
                // Do compensation logic, mail user, ...
            }
        }

        public async Task Handle(ReserveStockCancelResponse message, IMessageHandlerContext context)
        {
            Console.WriteLine($"ReserveStockCancelResponse {message.ReserveId}");

            await this.MarkAsCompleteDelayed(context);
        }

        public async Task Handle(ReserveStockConfirmResponse message, IMessageHandlerContext context)
        {
            Console.WriteLine($"ReserveStockConfirmResponse {message.ReserveId}");
            if (message.Success)
            {
                // Send billing generation command
                // Send shipping command
                await this.MarkAsCompleteDelayed(context);
            }
            else
            {
                // Do compensation, cancel Payment
                throw new NotImplementedException();
            }
        }

        //public async Task Handle(ShippingCompletedEvent message, IMessageHandlerContext context)
        //{
        //    Console.WriteLine($"ShippingCompletedEvent {message.OrderId}");
        //    this.MarkAsComplete();
        //}


        public async Task Timeout(MarkOrderSagaAsCompleteCleanupTimeout state, IMessageHandlerContext context)
        {
            await context.SendLocal(new MarkOrderSagaAsComplete { OrderId = Data.OrderId });
        }

        public Task Handle(MarkOrderSagaAsComplete message, IMessageHandlerContext context)
        {
            MarkAsComplete();
            return Task.CompletedTask;
        }

        private async Task MarkAsCompleteDelayed(IMessageHandlerContext context)
        {
            Data.MarkedForCompletion = true;
            await RequestTimeout(context, DateTimeOffset.UtcNow.AddMonths(1), new MarkOrderSagaAsCompleteCleanupTimeout { });
        }
    }
}
