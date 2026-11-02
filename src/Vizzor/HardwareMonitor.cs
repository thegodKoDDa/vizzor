using System;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace Vizzor;

public sealed class HardwareMetrics
{
    public float CpuTemp { get; set; }
    public float GpuTemp { get; set; }
    public float CpuLoad { get; set; }
    public float GpuLoad { get; set; }
    public float RamLoad { get; set; }
    public float MotherboardTemp { get; set; }
}

public sealed class HardwareMonitor : IDisposable
{
    private readonly Computer _computer;
    private bool _disposed;

    public HardwareMonitor()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true
        };

        try
        {
            _computer.Open();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to open hardware monitor: {ex.Message}");
        }
    }

    public HardwareMetrics GetMetrics()
    {
        var metrics = new HardwareMetrics();

        try
        {
            foreach (IHardware hardware in _computer.Hardware)
            {
                hardware.Update();

                switch (hardware.HardwareType)
                {
                    case HardwareType.Cpu:
                        ExtractCpuMetrics(hardware, metrics);
                        break;

                    case HardwareType.GpuNvidia:
                    case HardwareType.GpuAmd:
                    case HardwareType.GpuIntel:
                        ExtractGpuMetrics(hardware, metrics);
                        break;

                    case HardwareType.Memory:
                        ExtractMemoryMetrics(hardware, metrics);
                        break;

                    case HardwareType.Motherboard:
                        ExtractMotherboardMetrics(hardware, metrics);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading sensors: {ex.Message}");
        }

        return metrics;
    }

    private static void ExtractCpuMetrics(IHardware hardware, HardwareMetrics metrics)
    {
        // Temperatura
        var tempSensors = hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature).ToList();
        var preferredTemp = tempSensors.FirstOrDefault(s => s.Name.Contains("Core (Tctl/Tdie)", StringComparison.OrdinalIgnoreCase) && s.Value > 0)
                         ?? tempSensors.FirstOrDefault(s => s.Name.Contains("CCD", StringComparison.OrdinalIgnoreCase) && s.Value > 0)
                         ?? tempSensors.FirstOrDefault(s => s.Name.Contains("Package", StringComparison.OrdinalIgnoreCase) && s.Value > 0)
                         ?? tempSensors.FirstOrDefault(s => s.Value > 0)
                         ?? tempSensors.FirstOrDefault();

        if (preferredTemp?.Value is { } tempVal && tempVal > 0)
        {
            metrics.CpuTemp = tempVal;
        }

        // Carga
        var loadSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name.Contains("Total", StringComparison.OrdinalIgnoreCase))
                      ?? hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load);

        if (loadSensor?.Value is { } loadVal)
        {
            metrics.CpuLoad = loadVal;
        }
    }

    private static void ExtractGpuMetrics(IHardware hardware, HardwareMetrics metrics)
    {
        // Temperatura (GPU Core)
        var tempSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Name.Contains("Core", StringComparison.OrdinalIgnoreCase))
                      ?? hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);

        if (tempSensor?.Value is { } tempVal && tempVal > metrics.GpuTemp)
        {
            metrics.GpuTemp = tempVal;
        }

        // Carga GPU Core
        var loadSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name.Contains("Core", StringComparison.OrdinalIgnoreCase))
                      ?? hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load);

        if (loadSensor?.Value is { } loadVal && loadVal > metrics.GpuLoad)
        {
            metrics.GpuLoad = loadVal;
        }
    }

    private static void ExtractMemoryMetrics(IHardware hardware, HardwareMetrics metrics)
    {
        var loadSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name.Contains("Memory", StringComparison.OrdinalIgnoreCase))
                      ?? hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load);

        if (loadSensor?.Value is { } loadVal)
        {
            metrics.RamLoad = loadVal;
        }
    }

    private static void ExtractMotherboardMetrics(IHardware hardware, HardwareMetrics metrics)
    {
        var tempSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);
        if (tempSensor?.Value is { } tempVal)
        {
            metrics.MotherboardTemp = tempVal;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                _computer.Close();
            }
            catch { }
            _disposed = true;
        }
    }
}
