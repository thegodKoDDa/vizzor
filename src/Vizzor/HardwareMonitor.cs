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
        public string CpuTemperatureSensor { get; set; }
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

        // Diagnóstico de Driver de Kernel
        public bool IsPawnIoNeeded { get; set; }
    }

    public sealed class HardwareMonitor : IDisposable
    {
        public static bool IsPawnIoAvailable
        {
            get
            {
                try
                {
                    return File.Exists(@"C:\Program Files\PawnIO\PawnIO.sys") ||
                           File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"PawnIO\PawnIO.sys"));
                }
                catch { return false; }
            }
        }
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
        private readonly GpuMonitor _gpuMonitor;

        public HardwareMonitor()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = false,
                IsStorageEnabled = true,
                IsMemoryEnabled = false,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true
            };

            _gpuMonitor = new GpuMonitor();
            try
            {
                _computer.Open();
            }
            catch (Exception ex)
            {
                AppLog.Write("Computer.Open: " + ex);
            }
        }

        public HardwareMetrics GetMetrics()
        {
            var metrics = new HardwareMetrics();

            // 1. Memória RAM nativa do Windows (Win32 GlobalMemoryStatusEx - rápido e 100% confiável)
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

            // 2. Sensores de CPU, GPU, Storage e Placa-Mãe via LibreHardwareMonitor
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
                AppLog.Write("GetMetrics: " + ex);
            }

            _gpuMonitor.CopyTo(metrics);

            if (metrics.CpuTemp == 0 && !IsPawnIoAvailable)
            {
                metrics.IsPawnIoNeeded = true;
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
            int bestPriority = 0;
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
                    int priority = CpuTemperatureSelector.Priority(sensor.Name);
                    if (!float.IsNaN(val) && !float.IsInfinity(val) &&
                        (priority > bestPriority || (priority == bestPriority && val > bestTemp)))
                    {
                        bestTemp = val;
                        metrics.CpuTemperatureSensor = sensor.Name;
                        bestPriority = priority;
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

        internal static void ExtractGpuMetrics(IHardware hardware, HardwareMetrics metrics)
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

            // Placas-mãe costumam ter sensores no hardware principal ou em sub-hardware (chips Super I/O)
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
                _gpuMonitor.Dispose();
                try { _computer.Close(); } catch { }
                _disposed = true;
            }
        }
    }
}
