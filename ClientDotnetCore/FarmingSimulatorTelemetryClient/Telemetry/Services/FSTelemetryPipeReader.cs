using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using FarmingSimulatorTelemetryClient.PipeServer.Models.EventArgs;
using FarmingSimulatorTelemetryClient.PipeServer.Services;
using FarmingSimulatorTelemetryClient.Telemetry.Models;

namespace FarmingSimulatorTelemetryClient.Telemetry.Services;

public delegate void AfterTelemetryRead(FSTelemetryData telemetry);

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class FSTelemetryPipeReader
{
    public bool IsActive { get; private set; }
    public event AfterTelemetryRead AfterTelemetryRead;

    private FSTelemetryData TData { get; } = new();
    private FSNamedPipeReader PipeReader { get; } = new("fssimx");

    private Dictionary<string, PropertyInfo> _Properties = new();
    private readonly Dictionary<short, PropertyInfo> _Indexes = new();

    public FSTelemetryPipeReader(AfterTelemetryRead afterTelemetryRead)
    {
        InitializeProperties();
        PipeReader.OnReceiveMessageHandler += OnTelemetryReceived;
        AfterTelemetryRead += afterTelemetryRead;
    }

    public void Start()
    {
        PipeReader.Start();
        IsActive = true;
    }

    public void Stop()
    {
        PipeReader.Stop();
        IsActive = false;
    }

    private void InitializeProperties()
    {
        _Properties = new Dictionary<string, PropertyInfo>();
        var properties = TData.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
            _Properties.Add(property.Name.ToLower(), property);
    }

    private void OnTelemetryReceived(object? sender, MessageReceivedArgs args)
    {
        if (args.Message.StartsWith("HEADER"))
            ProcessIndexes(args.Message);
        else
            ProcessData(args.Message);
    }

    private void ProcessIndexes(string headersText)
    {
        var headers = headersText.Split('$');
        for (short i = 1; i < headers.Length - 1; i++)
        {
            if (!_Properties.TryGetValue(headers[i].ToLower(), out var prop))
            {
                Console.WriteLine($"Property [{headers[i]}] not indexed");
                if (_Indexes.ContainsKey(i))
                    _Indexes.Remove(i);
                continue;
            }

            _Indexes.TryAdd(i, prop);
        }
    }

    private void ProcessData(string dataText)
    {
        // Console.WriteLine(dataText); // <= Debug line
        var values = dataText.Split('$');
        for (short i = 1; i < values.Length - 1; i++)
        {
            if (!_Indexes.TryGetValue(i, out var prop))
                continue;

            var convertedValue = ConvertToType(prop.PropertyType, values[i]);
            if (convertedValue != null)
                prop.SetValue(TData, convertedValue);
        }

        if (IsActive)
            AfterTelemetryRead.Invoke(TData);
    }

    #region Internal Methods

    private static object? ConvertToType(Type type, string value)
    {
        switch (type)
        {
            // handle boolean
            case var _ when type == typeof(bool):
                return value.Trim() == "1";
            // handle int
            case var _ when type == typeof(int):
                return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var @int) ? @int : 0;
            // handle long
            case var _ when type == typeof(long):
                return long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var @long) ? @long : 0;
            // handle decimal
            case var _ when type == typeof(decimal):
                return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var @decimal) ? @decimal : 0m;
            // handle string
            case var _ when type == typeof(string):
                return value;
            // handle enum
            case var _ when type.IsEnum:
                return Enum.Parse(type, value); // TODO: Make TryParse
            // handle array
            case var _ when type.IsArray:
                var elmType = type.GetElementType();
                if (elmType == null) return null;
                var values = value.Split('¶');
                var array = Array.CreateInstance(elmType, values.Length - 1);
                for (var i = 0; i < values.Length - 1; i++)
                    array.SetValue(ConvertToType(elmType, values[i]), i);
                return array;
            // default
            default:
                return null;
        }
    }

    #endregion
}