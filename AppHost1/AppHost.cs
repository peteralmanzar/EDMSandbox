using Aspire.Hosting.ApplicationModel;

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
        var LocalCosmosDbEmulatorService = builder.AddConnectionString("localCosmosDbEmulator");

        // Function App
        var eventHub = eventHubs.AddHub("messages");
        var function1_AzureWebJobsStorage = azureStorage.AddBlobs($"function1-AzureWebJobsStorage");
        builder.AddProject<Projects.FunctionApp1>("FuncitonApp")
            .WaitFor(eventHubs)
            .WaitFor(function1_AzureWebJobsStorage)
            .WithReference(eventHub, "EDMEventHub")
            .WithReference(LocalCosmosDbEmulatorService, connectionName: "CosmosDbConnection")
            .WithEnvironment("AzureWebJobsStorage", function1_AzureWebJobsStorage)
            .WithEnvironment("cosmosDbDatabase", "associate-app-azure-cosmosdb-database")
            .WithEnvironment("cosmosDbContainer", "Assignments")
            .WithEnvironment("Logging__LogLevel__Default", "Debug")                             // uncomment for verbose logging
            .WithEnvironment("Logging__LogLevel__Microsoft", "Debug");                           // uncomment for verbose logging

        builder.Build().Run();
    }
}