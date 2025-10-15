using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionApp1.Functions;
public class EventHubListenerFunction(ILogger<EventHubListenerFunction> logger)
{
    private readonly ILogger<EventHubListenerFunction> _logger = logger;

    [Function(nameof(EventHubListenerFunction))]
    [ExponentialBackoffRetry(5, "00:00:02", "00:00:10")]
    [CosmosDBOutput(databaseName: "cosmosDb-Database", containerName: "cosmosDb-Container", Connection = "CosmosDbConnection", CreateIfNotExists = true)]
    public object?[] Run([EventHubTrigger(eventHubName: "messages", Connection = "EDMEventHub")] EdmEvent[] events)
    {
        _logger.LogInformation($"Executed {nameof(EventHubListenerFunction)}");
        return [.. events.Select(x => new 
        {
            id = x.id,
            name = x.Name,
            message = x.Message,
            receivedAt = DateTime.UtcNow
        })];
    }
}