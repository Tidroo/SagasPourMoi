using System.Runtime.InteropServices;

namespace IntegrationTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        private const string ORDER_ID = "83CF5634-ED22-41AC-B7FF-DE7F4A989F67";

        [Test]
        public async Task SendPlaceOrderCommand()
        {
            var endpointConfiguration = new EndpointConfiguration("IntegrationTests");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();
            transport.StorageDirectory("C:\\Git\\MerciYanis\\LearningTransport");
                        
            endpointConfiguration.UsePersistence<LearningPersistence>();
            endpointConfiguration.SendOnly();
            var endpointInstance = await Endpoint.Start(endpointConfiguration);

            await endpointInstance.Send("OrderService", new OrderService.Messages.PlaceOrderCommand
            {
                OrderId = ORDER_ID,
                ProductsQuantity = new Dictionary<string, int> { { "Item A", 1 }, { "Item B", 10 } }
            });

            Assert.Pass();
        }

        [Test]
        public async Task SendSucceedPaymentCompletedResponse()
        {
            var endpointConfiguration = new EndpointConfiguration("IntegrationTests");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();
            transport.StorageDirectory("C:\\Git\\MerciYanis\\LearningTransport");

            endpointConfiguration.UsePersistence<LearningPersistence>();
            endpointConfiguration.SendOnly();
            var endpointInstance = await Endpoint.Start(endpointConfiguration);

            await endpointInstance.Send("OrderService", new OrderService.Messages.PaymentCompletionResponse
            {
                OrderId = ORDER_ID,
                Success = true
            });

            Assert.Pass();
        }


        [Test]
        public async Task SendFailedPaymentCompletedResponse()
        {
            var endpointConfiguration = new EndpointConfiguration("IntegrationTests");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();
            transport.StorageDirectory("C:\\Git\\MerciYanis\\LearningTransport");

            endpointConfiguration.UsePersistence<LearningPersistence>();
            endpointConfiguration.SendOnly();
            var endpointInstance = await Endpoint.Start(endpointConfiguration);

            await endpointInstance.Send("OrderService", new OrderService.Messages.PaymentCompletionResponse
            {
                OrderId = ORDER_ID,
                Success = false
            });

            Assert.Pass();
        }
    }
}