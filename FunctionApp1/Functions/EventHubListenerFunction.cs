using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FunctionApp1.Functions;
public class EventHubListenerFunction(ILogger<EventHubListenerFunction> logger)
{
    private readonly ILogger<EventHubListenerFunction> _logger = logger;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };


    [Function(nameof(EventHubListenerFunction))]
    [ExponentialBackoffRetry(5, "00:00:02", "00:00:10")]
    [CosmosDBOutput(databaseName: "%cosmosDbDatabase%", containerName: "%cosmosDbContainer%", Connection = "CosmosDbConnection", PartitionKey = "/id", CreateIfNotExists = false)]
    public IEnumerable<EdmEvent> Run([EventHubTrigger(eventHubName: "messages", Connection = "EDMEventHub", IsBatched = true)] string[] events)
    {
        List<EdmEvent> documents = [];

        foreach (var rawJson in events)
        {
            var edmEvent = JsonSerializer.Deserialize<EdmEvent>(rawJson, _jsonSerializerOptions);
            if(edmEvent is null)
            {
                _logger.LogWarning("Skipping event {rawJson}", rawJson);
                continue;
            }

            _logger.LogInformation("Parsed message from {User}", edmEvent.Name);
            documents.Add(edmEvent);
        }

        return documents;
    }
}