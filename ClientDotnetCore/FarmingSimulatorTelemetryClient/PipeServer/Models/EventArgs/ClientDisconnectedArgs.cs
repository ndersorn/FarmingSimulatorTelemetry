namespace FarmingSimulatorTelemetryClient.PipeServer.Models.EventArgs;

public class ClientDisconnectedArgs : System.EventArgs
{
    public required string ClientId { get; init; }
}