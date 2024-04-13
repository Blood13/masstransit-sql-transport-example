namespace Example.Contracts;

public class Message
{
    public string PartitionKey { get; init; } = null!;
    public string Data { get; init; } = null!;
}