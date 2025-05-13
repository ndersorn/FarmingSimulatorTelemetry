using Avalonia.Controls;
using Avalonia.Interactivity;
using FarmingSimulatorTelemetryClient.Telemetry.Models;
using FarmingSimulatorTelemetryClient.Telemetry.Services;
using Newtonsoft.Json;

namespace FarmingSimulatorTelemetryClient;

public partial class MainWindow : Window
{
    /// <summary>
    /// Telemetry Pipe Reader
    /// </summary>
    private FSTelemetryPipeReader? TelemetryReader { get; set; }

    public MainWindow()
    {
        InitializeComponent();
    }

    ~MainWindow() =>
        TelemetryReader?.Stop();

    private void btnStart_Click(object? sender, RoutedEventArgs e)
    {
        // stop if already started
        TelemetryReader?.Stop();
        // setup TelemetryReader
        TelemetryReader = new FSTelemetryPipeReader(OnUpdateView);
        // start reader
        TelemetryReader.Start();
    }

    private void btnStop_Click(object? sender, RoutedEventArgs e) =>
        TelemetryReader?.Stop();

    private void OnUpdateView(FSTelemetryData telemetry)
    {
        // serialize object as user readable
        var pDat = JsonConvert.SerializeObject(telemetry.GameData, Formatting.Indented);
        var vDat = JsonConvert.SerializeObject(telemetry.VehicleData, Formatting.Indented);

        // output to rich-text box
        PlayerDataText.Text = pDat;
        VehicleDataText.Text = vDat;
    }
}