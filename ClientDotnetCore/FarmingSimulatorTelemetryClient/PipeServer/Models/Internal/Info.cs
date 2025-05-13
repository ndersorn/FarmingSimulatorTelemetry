using System.Text;

namespace FarmingSimulatorTelemetryClient.PipeServer.Models.Internal;

public class Info(int bufferSize)
{
    public readonly byte[] Buffer = new byte[bufferSize];
    public readonly StringBuilder StringBuilder = new();
}