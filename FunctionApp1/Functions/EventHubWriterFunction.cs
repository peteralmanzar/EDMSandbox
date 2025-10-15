using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionApp1.Functions;
public class EventHubWriterFunction(ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<EventHubWriterFunction>();

    // {second} {minute} {hour} {day} {month} {day-of-week}
    [EventHubOutput(eventHubName: "messages", Connection = "EDMEventHub")]
    [Function(nameof(EventHubWriterFunction))]
    public EdmEvent Run([TimerTrigger("0 */1 * * * *", RunOnStartup = true)] TimerInfo timerInfo)
    {
        var result = EdmEventGenerator.Faker.Generate();
        return result;
    }
}