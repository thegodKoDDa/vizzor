using System;
using System.IO;
using System.Runtime.InteropServices;
using LibreHardwareMonitor.Hardware;

namespace Vizzor
{
    public sealed class HardwareMetrics
    {
        // CPU
        public string CpuName { get; set; }
        public float CpuTemp { get; set; }
        public float CpuLoad { get; set; }
        public float CpuClockGhz { get; set; }
        public float CpuPowerWatts { get; set; }
        public float CpuVoltage { get; set; }

        // GPU
        public string GpuName { get; set; }
        public float GpuTemp { get; set; }
        public float GpuHotspotTemp { get; set; }
        public float GpuLoad { get; set; }
        public float GpuVramUsedGb { get; set; }
        public float GpuVramTotalGb { get; set; }
        public float GpuPowerWatts { get; set; }
        public float GpuFanRpm { get; set; }

        // Memória RAM
        public float RamLoad { get; set; }
        public float RamUsedGb { get; set; }
        public float RamTotalGb { get; set; }

        // Armazenamento
        public string StorageName { get; set; }
        public float SsdTemp { get; set; }

        // Placa-Mãe e Ventoinhas
        public string MotherboardName { get; set; }
        public float MotherboardTemp { get; set; }
        public float CpuFanRpm { get; set; }
    }

    public sealed class HardwareMonitor : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        private readonly Computer _computer;
        private bool _disposed;

        public HardwareMonitor()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsStorageEnabled = true,
                IsMemoryEnabled = false,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true
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

            // 1. Memória RAM nativa do Windows (Win32 GlobalMemoryStatusEx)
            try
            {
                var mem = new MEMORYSTATUSEX();
                mem.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                if (GlobalMemoryStatusEx(ref mem))
                {
                    metrics.RamLoad = mem.dwMemoryLoad;
                    metrics.RamTotalGb = (float)Math.Round((double)mem.ullTotalPhys / (1024.0 * 1024.0 * 1024.0), 1);
                    double usedBytes = (double)(mem.ullTotalPhys - mem.ullAvailPhys);
                    metrics.RamUsedGb = (float)Math.Round(usedBytes / (1024.0 * 1024.0 * 1024.0), 1);
                }
            }
            catch { }

            // 2. Sensores via LibreHardwareMonitor
            try
            {
                foreach (IHardware hardware in _computer.Hardware)
                {
                    try
                    {
                        hardware.Update();

                        if (hardware.HardwareType == HardwareType.Cpu)
                        {
                            ExtractCpuMetrics(hardware, metrics);
                        }
                        else if (hardware.HardwareType == HardwareType.GpuNvidia ||
                                 hardware.HardwareType == HardwareType.GpuAmd ||
                                 hardware.HardwareType == HardwareType.GpuIntel)
                        {
                            ExtractGpuMetrics(hardware, metrics);
                        }
                        else if (hardware.HardwareType == HardwareType.Storage)
                        {
                            ExtractStorageMetrics(hardware, metrics);
                        }
                        else if (hardware.HardwareType == HardwareType.Motherboard)
                        {
                            ExtractMotherboardMetrics(hardware, metrics);
                        }
                    }
                    catch { }
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
            if (string.IsNullOrEmpty(metrics.CpuName))
            {
                metrics.CpuName = CleanHardwareName(hardware.Name);
            }

            float bestTemp = 0;
            float totalLoad = 0;
            float maxClock = 0;
            float powerWatts = 0;
            float voltage = 0;

            foreach (var sensor in hardware.Sensors)
            {
                if (!sensor.Value.HasValue) continue;
                float val = sensor.Value.Value;

                if (sensor.SensorType == SensorType.Temperature && val > 0)
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
                else if (sensor.SensorType == SensorType.Load)
                {
                    if (sensor.Name.IndexOf("Total", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        totalLoad = val;
                    }
                    else if (totalLoad == 0)
                    {
                        totalLoad = val;
                    }
                }
                else if (sensor.SensorType == SensorType.Clock && val > 0)
                {
                    if (val > maxClock) maxClock = val;
                }
                else if (sensor.SensorType == SensorType.Power && val > 0)
                {
                    if (sensor.Name.IndexOf("Package", StringComparison.OrdinalIgnoreCase) >= 0 || powerWatts == 0)
                    {
                        powerWatts = val;
                    }
                }
                else if (sensor.SensorType == SensorType.Voltage && val > 0)
                {
                    if (sensor.Name.IndexOf("Core", StringComparison.OrdinalIgnoreCase) >= 0 || voltage == 0)
                    {
                        voltage = val;
                    }
                }
            }

            metrics.CpuTemp = bestTemp;
            metrics.CpuLoad = totalLoad;
            if (maxClock > 0) metrics.CpuClockGhz = (float)Math.Round(maxClock / 1000.0, 2);
            metrics.CpuPowerWatts = (float)Math.Round(powerWatts, 1);
            metrics.CpuVoltage = (float)Math.Round(voltage, 2);
        }

        private static void ExtractGpuMetrics(IHardware hardware, HardwareMetrics metrics)
        {
            if (string.IsNullOrEmpty(metrics.GpuName))
            {
                metrics.GpuName = CleanHardwareName(hardware.Name);
            }

            foreach (var sensor in hardware.Sensors)
            {
                if (!sensor.Value.HasValue) continue;
                float val = sensor.Value.Value;

                if (sensor.SensorType == SensorType.Temperature && val > 0)
                {
                    if (sensor.Name.IndexOf("Hot", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        metrics.GpuHotspotTemp = val;
                    }
                    else if (sensor.Name.IndexOf("Core", StringComparison.OrdinalIgnoreCase) >= 0 || metrics.GpuTemp == 0)
                    {
                        metrics.GpuTemp = val;
                    }
                }
                else if (sensor.SensorType == SensorType.Load)
                {
                    if (sensor.Name.IndexOf("Core", StringComparison.OrdinalIgnoreCase) >= 0 || metrics.GpuLoad == 0)
                    {
                        metrics.GpuLoad = val;
                    }
                }
                else if (sensor.SensorType == SensorType.Power && val > 0)
                {
                    if (sensor.Name.IndexOf("Board", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        sensor.Name.IndexOf("Package", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        metrics.GpuPowerWatts == 0)
                    {
                        metrics.GpuPowerWatts = (float)Math.Round(val, 1);
                    }
                }
                else if (sensor.SensorType == SensorType.Fan && val > 0)
                {
                    metrics.GpuFanRpm = val;
                }
                else if (sensor.SensorType == SensorType.SmallData || sensor.SensorType == SensorType.Data)
                {
                    if (sensor.Name.IndexOf("Memory Used", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        metrics.GpuVramUsedGb = val > 100 ? (float)Math.Round(val / 1024.0, 1) : (float)Math.Round(val, 1);
                    }
                    else if (sensor.Name.IndexOf("Memory Total", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        metrics.GpuVramTotalGb = val > 100 ? (float)Math.Round(val / 1024.0, 1) : (float)Math.Round(val, 1);
                    }
                }
            }
        }

        private static void ExtractStorageMetrics(IHardware hardware, HardwareMetrics metrics)
        {
            if (string.IsNullOrEmpty(metrics.StorageName))
            {
                metrics.StorageName = CleanHardwareName(hardware.Name);
            }

            foreach (var sensor in hardware.Sensors)
            {
                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                {
                    float val = sensor.Value.Value;
                    if (val > 0 && (val > metrics.SsdTemp || metrics.SsdTemp == 0))
                    {
                        metrics.SsdTemp = val;
                    }
                }
            }
        }

        private static void ExtractMotherboardMetrics(IHardware hardware, HardwareMetrics metrics)
        {
            if (string.IsNullOrEmpty(metrics.MotherboardName))
            {
                metrics.MotherboardName = CleanHardwareName(hardware.Name);
            }

            InspectSensorsForMotherboard(hardware, metrics);

            foreach (var sub in hardware.SubHardware)
            {
                try
                {
                    sub.Update();
                    InspectSensorsForMotherboard(sub, metrics);
                }
                catch { }
            }
        }

        private static void InspectSensorsForMotherboard(IHardware hardware, HardwareMetrics metrics)
        {
            foreach (var sensor in hardware.Sensors)
            {
                if (!sensor.Value.HasValue) continue;
                float val = sensor.Value.Value;

                if (sensor.SensorType == SensorType.Temperature && val > 0)
                {
                    if (metrics.MotherboardTemp == 0 || sensor.Name.IndexOf("System", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        metrics.MotherboardTemp = val;
                    }
                }
                else if (sensor.SensorType == SensorType.Fan && val > 0)
                {
                    if (metrics.CpuFanRpm == 0 || sensor.Name.IndexOf("CPU", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        metrics.CpuFanRpm = val;
                    }
                }
            }
        }

        private static string CleanHardwareName(string rawName)
        {
            if (string.IsNullOrEmpty(rawName)) return "";
            string name = rawName.Trim();
            if (name.StartsWith("Generic ", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(8);
            return name;
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
