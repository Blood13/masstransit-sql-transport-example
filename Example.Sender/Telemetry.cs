using System.Diagnostics;

namespace Example.Sender;

public static class Telemetry
{
    public static readonly ActivitySource SenderActivitySource = new("Sender");
}
