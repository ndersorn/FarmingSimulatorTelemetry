using System.Diagnostics.CodeAnalysis;
using FarmingSimulatorTelemetryClient.Telemetry.Enums;

namespace FarmingSimulatorTelemetryClient.Telemetry.Models.SubModels;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class FSGameData
{
    public decimal Money { get; set; }
    public decimal TemperatureMin { get; set; }
    public decimal TemperatureMax { get; set; }
    public TemperatureTrendType TemperatureTrend { get; set; }
    public int DayTimeMinutes { get; set; }
    public WeatherType WeatherCurrent { get; set; }
    public WeatherType WeatherNext { get; set; }
    public int Day { get; set; }
    public GameEditionType GameEdition { get; set; }
}