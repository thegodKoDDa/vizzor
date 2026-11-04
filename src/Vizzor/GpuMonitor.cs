using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using LibreHardwareMonitor.Hardware;

namespace Vizzor
{
    // NVML pode causar AccessViolation nativa. Um processo separado impede que
    // essa falha derrube os sensores de CPU, a bandeja e o envio USB.
    internal sealed class GpuMonitor : IDisposable
    {
        private readonly object _sync = new object();
        private readonly ManualResetEvent _stop = new ManualResetEvent(false);
        private Process _process;
        private HardwareMetrics _latest = new HardwareMetrics();
        private DateTime _lastSample = DateTime.MinValue;
        private bool _disposed;

        internal GpuMonitor()
        {
            new Thread(ReadWorker) { IsBackground = true, Name = "Vizzor GPU" }.Start();
        }

        private void ReadWorker()
        {
            for (int attempt = 0; attempt < 3 && !_stop.WaitOne(0); attempt++)
            {
                try
                {
                    using (var process = new Process())
                    {
                        process.StartInfo = new ProcessStartInfo
                        {
                            FileName = System.Windows.Forms.Application.ExecutablePath,
                            Arguments = "--gpu-worker",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                        };
                        lock (_sync)
                        {
                            if (_disposed) return;
                            process.Start();
                            _process = process;
                        }
                        using (var reader = new BinaryReader(process.StandardOutput.BaseStream))
                        {
                            while (!_stop.WaitOne(0))
                            {
                                var sample = new HardwareMetrics
                                {
                                    GpuName = reader.ReadString(),
                                    GpuTemp = reader.ReadSingle(),
                                    GpuHotspotTemp = reader.ReadSingle(),
                                    GpuLoad = reader.ReadSingle(),
                                    GpuVramUsedGb = reader.ReadSingle(),
                                    GpuVramTotalGb = reader.ReadSingle(),
                                    GpuPowerWatts = reader.ReadSingle(),
                                    GpuFanRpm = reader.ReadSingle()
                                };
                                lock (_sync) { _latest = sample; _lastSample = DateTime.UtcNow; }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!_stop.WaitOne(0)) AppLog.Write("GPU worker interrompido: " + ex.Message);
                }
                finally
                {
                    lock (_sync) { _process = null; _lastSample = DateTime.MinValue; }
                }
                if (_stop.WaitOne(10000)) return;
            }
            if (!_stop.WaitOne(0)) AppLog.Write("GPU indisponivel apos 3 tentativas; CPU e USB continuam ativos.");
        }

        internal void CopyTo(HardwareMetrics target)
        {
            lock (_sync)
            {
                if ((DateTime.UtcNow - _lastSample).TotalSeconds > 5) return;
                target.GpuName = _latest.GpuName;
                target.GpuTemp = _latest.GpuTemp;
                target.GpuHotspotTemp = _latest.GpuHotspotTemp;
                target.GpuLoad = _latest.GpuLoad;
                target.GpuVramUsedGb = _latest.GpuVramUsedGb;
                target.GpuVramTotalGb = _latest.GpuVramTotalGb;
                target.GpuPowerWatts = _latest.GpuPowerWatts;
                target.GpuFanRpm = _latest.GpuFanRpm;
            }
        }

        internal static void RunWorker()
        {
            var computer = new Computer { IsGpuEnabled = true };
            try
            {
                using (var writer = new BinaryWriter(Console.OpenStandardOutput()))
                {
                    computer.Open();
                    while (true)
                    {
                        var sample = new HardwareMetrics();
                        foreach (IHardware hardware in computer.Hardware)
                        {
                            hardware.Update();
                            HardwareMonitor.ExtractGpuMetrics(hardware, sample);
                        }
                        writer.Write(sample.GpuName ?? "");
                        writer.Write(sample.GpuTemp);
                        writer.Write(sample.GpuHotspotTemp);
                        writer.Write(sample.GpuLoad);
                        writer.Write(sample.GpuVramUsedGb);
                        writer.Write(sample.GpuVramTotalGb);
                        writer.Write(sample.GpuPowerWatts);
                        writer.Write(sample.GpuFanRpm);
                        writer.Flush();
                        Thread.Sleep(1000);
                    }
                }
            }
            finally { computer.Close(); }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                _disposed = true;
                _stop.Set();
                try { if (_process != null && !_process.HasExited) _process.Kill(); } catch { }
            }
        }
    }
}
