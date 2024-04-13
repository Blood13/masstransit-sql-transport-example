using Example.Contracts;
using MassTransit;

namespace Example.Sender;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IBus _bus;

    public Worker(ILogger<Worker> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var myActivity = Telemetry.SenderActivitySource.StartActivity("SendMessages");

        var endpoint = await _bus.GetPublishSendEndpoint<Message>();
        var messages = MaxParallelSpread(10, 5);

        foreach (var batch in messages.Chunk(2500)) // in case of huge amount of messages
        {
            await endpoint.SendBatch(batch, c =>
            {
                c.SetPartitionKey(c.Message.PartitionKey);
            }, stoppingToken);
        }

        _logger.LogInformation("Messages are sent");
    }

    private List<Message> MaxParallelSpread(int partitionsCount, int messagesPerPartitionCount)
    {
        var messages = new List<Message>(partitionsCount * messagesPerPartitionCount);

        for (var messageNumber = 0; messageNumber < messagesPerPartitionCount; messageNumber++)
        {
            for (var partitionNumber = 1; partitionNumber <= partitionsCount; partitionNumber++)
            {
                var message = new Message
                {
                    PartitionKey = partitionNumber.ToString(),
                    Data = "important business data"
                };

                messages.Add(message);
            }
        }

        return messages;
    }
}