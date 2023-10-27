
IHost host = Host.CreateDefaultBuilder(args)
    .UseNServiceBus(builder =>
    {
        var endpointConfiguration = new EndpointConfiguration("StockService");
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
