using System.Diagnostics.CodeAnalysis;
using FarmingSimulatorTelemetryClient.Telemetry.Models.SubModels;

namespace FarmingSimulatorTelemetryClient.Telemetry.Models;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class FSTelemetryData
{
    public FSGameData GameData { get; } = new FSGameData();
    public FSVehicleData VehicleData { get; } = new FSVehicleData();
}