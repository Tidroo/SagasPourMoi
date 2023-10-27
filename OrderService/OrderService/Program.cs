
using System.Security.Cryptography.Xml;

IHost host = Host.CreateDefaultBuilder(args)
    .UseNServiceBus(builder =>
    {
        var endpointConfiguration = new EndpointConfiguration("OrderService");
        var transport = endpointConfiguration.UseTransport<LearningTransport>();
        transport.StorageDirectory("C:\\Git\\MerciYanis\\LearningTransport");
        var persistence = endpointConfiguration.UsePersistence<LearningPersistence>();
        persistence.SagaStorageDirectory("C:\\Git\\MerciYanis\\LearningPeristence");
        
        //endpointConfiguration.EnableOutbox();

        return endpointConfiguration;
    })
    .ConfigureServices(services =>
    {
    })
    .Build();

host.Run();
