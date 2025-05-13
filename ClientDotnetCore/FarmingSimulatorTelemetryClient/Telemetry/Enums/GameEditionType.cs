using System.Diagnostics.CodeAnalysis;

namespace FarmingSimulatorTelemetryClient.Telemetry.Enums;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum GameEditionType : short
{
    FS19 = 19,
    FS22 = 22,
    FS25 = 25
}