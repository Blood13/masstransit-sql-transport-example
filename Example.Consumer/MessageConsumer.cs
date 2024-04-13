using Example.Contracts;
using MassTransit;
using System.Diagnostics;

namespace Example.Consumer;

public sealed class MessageConsumer : IConsumer<Message>
{
    private readonly ILogger<MessageConsumer> _logger;

    public MessageConsumer(ILogger<MessageConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<Message> context)
    {
        var activity = Activity.Current;
        activity?.SetTag("PartitionKey", context.Message.PartitionKey);

        var rnd = new Random();
        var processingTime = rnd.Next(500, 1000);

        await Task.Delay(processingTime);
    }
}