using System;
using System.IO;
using System.Runtime.InteropServices;
using LibreHardwareMonitor.Hardware;

namespace Vizzor
{
    public sealed class HardwareMetrics
    {
        public float CpuTemp { get; set; }
        public float GpuTemp { get; set; }
        public float SsdTemp { get; set; }
        public float CpuLoad { get; set; }
        public float GpuLoad { get; set; }
        public float RamLoad { get; set; }
        public float MotherboardTemp { get; set; }
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
                IsMotherboardEnabled = false
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

            // 1. Memória RAM nativa do Windows (rápido e 100% confiável)
            try
            {
                var mem = new MEMORYSTATUSEX();
                mem.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                if (GlobalMemoryStatusEx(ref mem))
                {
                    metrics.RamLoad = mem.dwMemoryLoad;
                }
            }
            catch { }

            // 2. Sensores de CPU e GPU via LibreHardwareMonitor
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
                    else if (totalLoad == 0)
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
                    if (val > 0)
                    {
                        if (sensor.Name.IndexOf("Core", StringComparison.OrdinalIgnoreCase) >= 0 || metrics.GpuTemp == 0)
                        {
                            metrics.GpuTemp = val;
                        }
                    }
                }
                else if (sensor.SensorType == SensorType.Load && sensor.Value.HasValue)
                {
                    if (sensor.Name.IndexOf("Core", StringComparison.OrdinalIgnoreCase) >= 0 || metrics.GpuLoad == 0)
                    {
                        metrics.GpuLoad = sensor.Value.Value;
                    }
                }
            }
        }

        private static void ExtractStorageMetrics(IHardware hardware, HardwareMetrics metrics)
        {
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
