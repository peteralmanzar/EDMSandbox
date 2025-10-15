internal class Program
{
    private static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        // Azure Event Hubs
        var eventHubs = builder.AddAzureEventHubs("event-hubs")
            .RunAsEmulator(emulator =>
            {
                emulator.WithContainerName("EDMSandbox-EventHub");
            });

        // azure blob storage
        var azureStorage = builder.AddAzureStorage("azure-blob-storage")
            .RunAsEmulator(azurite =>
            {
                azurite.WithContainerName("azure-blob-storage");
            });

        // cosmos db
        var cosmosDb = builder.AddAzureCosmosDB("cosmos-db")
            .RunAsEmulator(emulator =>
            {
                emulator.WithContainerName("EDMSandbox-CosmosDb");

                emulator
                    .WithGatewayPort(5001)
                    .WithLifetime(ContainerLifetime.Persistent)
                    .WithPartitionCount(5)
                    .WithDataVolume("EDMSandbox-CosmosDb-Volume");
            })
            .WithUrl($"https://localhost:5001/_explorer/index.html", "CosmosDb Explorer");

        // Cosmos DB database and container
        var cosmosDbDatabase = cosmosDb.AddCosmosDatabase("cosmosDb-Database");
        cosmosDbDatabase.AddContainer("cosmosDb-Container", "/id");

        var eventHub = eventHubs.AddHub("messages");

        // Function App
        var function1_AzureWebJobsStorage = azureStorage.AddBlobs($"function1-AzureWebJobsStorage");
        builder.AddProject<Projects.FunctionApp1>("FuncitonApp")
            .WaitFor(eventHubs)
            .WaitFor(function1_AzureWebJobsStorage)
            .WaitFor(cosmosDb)
            .WithReference(eventHub, "EDMEventHub")
            .WithReference(cosmosDb, connectionName: "CosmosDbConnection")
            .WithEnvironment("AzureWebJobsStorage", function1_AzureWebJobsStorage)
            .WithEnvironment("Logging__LogLevel__Default", "Debug")                             // uncomment for verbose logging
            .WithEnvironment("Logging__LogLevel__Microsoft", "Debug");                           // uncomment for verbose logging

        builder.Build().Run();
    }
}