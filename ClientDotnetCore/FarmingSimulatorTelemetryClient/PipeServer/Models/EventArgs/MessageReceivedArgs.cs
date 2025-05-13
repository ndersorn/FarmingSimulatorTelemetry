namespace FarmingSimulatorTelemetryClient.PipeServer.Models.EventArgs;

public class MessageReceivedArgs : System.EventArgs
{
    public required string Message { get; init; }
}