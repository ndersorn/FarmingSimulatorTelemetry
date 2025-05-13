using System;

namespace FarmingSimulatorTelemetryClient.PipeServer.Utilities.Extensions;

public static class EventHandlerExtension
{
    public static void SafeInvoke<T>(this EventHandler<T>? @event, object sender, T args) where T : EventArgs =>
        @event?.Invoke(sender, args);
}