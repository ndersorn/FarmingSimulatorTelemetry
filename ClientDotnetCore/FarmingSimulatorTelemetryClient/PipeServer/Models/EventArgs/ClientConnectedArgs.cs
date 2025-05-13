namespace FarmingSimulatorTelemetryClient.PipeServer.Models.EventArgs;

public class ClientConnectedArgs : System.EventArgs
{
    public required string ClientId { get; init; }
}