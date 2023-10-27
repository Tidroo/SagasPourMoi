using StockService.Messages;

namespace StockService.Sagas
{
    public class ReserveSaga : Saga<ReserveSagaData>, 
        IAmStartedByMessages<ReserveStockCommand>,
        IHandleTimeouts<ReserveTimeout>,
        IHandleTimeouts<MarkReserveSagaAsCompleteCleanupTimeout>,
        IHandleMessages<ReserveStockConfirmCommand>,
        IHandleMessages<ReserveStockCancelCommand>,
        IHandleMessages<ReserveStockCancelResponse>,
        IHandleMessages<MarkReserveSagaAsComplete>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ReserveSagaData> mapper)
        {
            mapper.MapSaga(saga => saga.ReserveId)
                .ToMessage<ReserveStockCommand>(s => s.ReserveId)
                .ToMessage<ReserveStockConfirmCommand>(s => s.ReserveId)
                .ToMessage<ReserveStockCancelCommand>(s => s.ReserveId)
                .ToMessage<ReserveStockCancelResponse>(s => s.ReserveId)
                .ToMessage<MarkReserveSagaAsComplete>(s => s.ReserveId);
        }

        public async Task Handle(ReserveStockCommand message, IMessageHandlerContext context)
        {
            Console.WriteLine($"ReserveStockCommand {message.OrderId}");

            // TODO Deduplicate message
            //if (Data.ReserveId == message.ReserveId)
            //    return;

            Data.ProductsQuantity = message.ProductsQuantity;
            Data.OrderId = message.OrderId;
            //Data.Created 

            /*
             * Verify stock is enough and reserve it
             * 
             * This section is critical as we must avoid concurency race conditions when 2 process attempt to reserve stock
             *  Solution : Pessimistic lock
             *      A global lock (local or distributed) the entire critical section
             *      but it will reduce the parallelism capability of the process and create a bottleneck
             *  Solution: Optimistic lock
             *      The database item is time or version stamped and the update query verify that the current stamp match the DB stamp
             *      If no row is updated due to the check then rollback and retry the entire action
             */
            var stockIsEnough = true;

            /*
             * Outbox pattern
             * 
             * Here the outbox pattern is very important to avoid any lost messages
             * (The application DB and NServiceBus persistence layer should be the same)
             */
            if (stockIsEnough)
            {
                // In order to not block a Reserve for a too long period (ex 1h) request a
                await RequestTimeout(context, TimeSpan.FromMinutes(1), new ReserveTimeout { });

                await context.Publish(new StockReservedEvent
                {
                    ReserveId = message.ReserveId,
                    ProductsQuantity = message.ProductsQuantity
                });

                await context.Reply(new ReserveStockResponse
                {
                    ReserveId = message.ReserveId,
                    OrderId = message.OrderId,
                    Success = true
                });
            }
            else
            {
                await context.Reply(new ReserveStockResponse
                {
                    ReserveId = message.ReserveId,
                    OrderId = message.OrderId,
                    Success = false
                });

                await MarkAsCompleteDelayed(context);
            }
        }

        public async Task Timeout(ReserveTimeout state, IMessageHandlerContext context)
        {
            Console.WriteLine($"ReserveTimeout {Data.ReserveId}");
            if (Data.ReserveConfirmed || Data.ReserveCanceled)
                return;
            Data.ReserveTimedOut = true;
            // Cancel on timeout
            await context.SendLocal(new ReserveStockCancelCommand { ReserveId = Data.ReserveId });
        }

        public Task Handle(ReserveStockCancelResponse message, IMessageHandlerContext context)
        {
            Console.WriteLine($"ReserveStockCancelResponse {message.ReserveId}");
            return Task.CompletedTask;
        }

        public async Task Handle(ReserveStockConfirmCommand message, IMessageHandlerContext context)
        {
            Console.WriteLine($"ReserveStockConfirmCommand {message.ReserveId}");

            // Deduplication
            if (Data.ReserveConfirmed)
                return;

            if (Data.ReserveTimedOut || Data.ReserveCanceled)
            {
                await context.Reply(new ReserveStockConfirmResponse
                {
                    ReserveId = message.ReserveId,
                    OrderId = Data.OrderId,
                    Success = false
                });

                //await MarkAsCompleteDelayed(context); 
                return;
            }
            
            /*
             * This part should be well documented that a "Closed" event do not mean the stock decreased
             */
            // context.Publish<ReserveStockClosedEvent>()
            foreach(var (product, qty) in Data.ProductsQuantity)
                await context.SendLocal(new StockDecreaseCommand() { 
                    ProductId = product,
                    Quantity = qty
                });

            await context.Publish(new ReserveStockClosedEvent
            {
                ReserveId = message.ReserveId,
                ProductsQuantity = Data.ProductsQuantity
            });

            await context.Reply(new ReserveStockConfirmResponse
            {
                ReserveId = message.ReserveId,
                OrderId = Data.OrderId,
                Success = true
            });

            Data.ReserveConfirmed = true;
            await MarkAsCompleteDelayed(context);
        }

        public async Task Handle(ReserveStockCancelCommand message, IMessageHandlerContext context)
        {
            Console.WriteLine($"ReserveStockCancelCommand {message.ReserveId}");
            if (Data.ReserveConfirmed || Data.ReserveCanceled)
                return;

            await context.Reply(new ReserveStockCancelResponse 
            { 
                ReserveId = message.ReserveId,
                OrderId = Data.OrderId
            });

            // Reserve was cancelled from another path
            if(context.ReplyToAddress != Data.Originator)
                await context.Send(Data.Originator, new ReserveStockCancelMessage 
                { 
                    ReserveId = message.ReserveId,
                    OrderId = Data.OrderId
                });

            await context.Publish(new ReserveStockClosedEvent {
                ReserveId = message.ReserveId,
                ProductsQuantity = Data.ProductsQuantity 
            });

            Data.ReserveCanceled = true;
            await MarkAsCompleteDelayed(context);
        }

        public async Task Timeout(MarkReserveSagaAsCompleteCleanupTimeout state, IMessageHandlerContext context)
        {
            await context.SendLocal(new MarkReserveSagaAsComplete { ReserveId = Data.ReserveId });
        }

        public Task Handle(MarkReserveSagaAsComplete message, IMessageHandlerContext context)
        {
            MarkAsComplete();
            return Task.CompletedTask;
        }

        private async Task MarkAsCompleteDelayed(IMessageHandlerContext context)
        {
            Data.MarkedForCompletion = true;
            await RequestTimeout(context, DateTimeOffset.UtcNow.AddDays(1), new MarkReserveSagaAsCompleteCleanupTimeout { });
        }
    }
}
