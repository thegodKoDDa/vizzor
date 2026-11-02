using System;
using System.IO;
using LibreHardwareMonitor.Hardware;

namespace Vizzor
{
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
                try { File.AppendAllText(@"C:\Vizzor\vizzor.log", "[Error] Computer.Open: " + ex.Message + "\n"); } catch { }
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
                try { File.AppendAllText(@"C:\Vizzor\vizzor.log", "[Error] GetMetrics: " + ex.Message + "\n"); } catch { }
            }

            return metrics;
        }

        private static void ExtractCpuMetrics(IHardware hardware, HardwareMetrics metrics)
        {
            float bestTemp = 0;
            float totalLoad = 0;

            foreach (var sensor in hardware.Sensors)
            {
                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                {
                    float val = sensor.Value.Value;
                    if (val > 0)
                    {
                        if (sensor.Name.IndexOf("Tctl/Tdie", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            bestTemp = val;
                        }
                        else if (bestTemp == 0 && sensor.Name.IndexOf("CCD", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            bestTemp = val;
                        }
                        else if (bestTemp == 0 && sensor.Name.IndexOf("Package", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            bestTemp = val;
                        }
                        else if (bestTemp == 0)
                        {
                            bestTemp = val;
                        }
                    }
                }
                else if (sensor.SensorType == SensorType.Load && sensor.Value.HasValue)
                {
                    if (sensor.Name.IndexOf("Total", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        totalLoad = sensor.Value.Value;
                    }
                }
            }

            metrics.CpuTemp = bestTemp;
            metrics.CpuLoad = totalLoad;
        }

        private static void ExtractGpuMetrics(IHardware hardware, HardwareMetrics metrics)
        {
            foreach (var sensor in hardware.Sensors)
            {
                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                {
                    float val = sensor.Value.Value;
                    if (val > 0 && (sensor.Name.IndexOf("Core", StringComparison.OrdinalIgnoreCase) >= 0 || metrics.GpuTemp == 0))
                    {
                        metrics.GpuTemp = val;
                    }
                }
                else if (sensor.SensorType == SensorType.Load && sensor.Value.HasValue)
                {
                    if (sensor.Name.IndexOf("Core", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        metrics.GpuLoad = sensor.Value.Value;
                    }
                }
            }
        }

        private static void ExtractMemoryMetrics(IHardware hardware, HardwareMetrics metrics)
        {
            foreach (var sensor in hardware.Sensors)
            {
                if (sensor.SensorType == SensorType.Load && sensor.Value.HasValue)
                {
                    if (sensor.Name.IndexOf("Memory", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        metrics.RamLoad = sensor.Value.Value;
                    }
                }
            }
        }

        private static void ExtractMotherboardMetrics(IHardware hardware, HardwareMetrics metrics)
        {
            foreach (var sensor in hardware.Sensors)
            {
                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                {
                    metrics.MotherboardTemp = sensor.Value.Value;
                    break;
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try { _computer.Close(); } catch { }
                _disposed = true;
            }
        }
    }
}
