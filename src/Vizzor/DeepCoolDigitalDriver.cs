using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace Vizzor
{
    public sealed class DeepCoolDigitalDriver : IDisplayDevice
    {
        public string DeviceName { get { return "DeepCool Digital Series (AK400 / AK620 / LS720)"; } }
        public string Manufacturer { get { return "DeepCool Industries"; } }
        public ushort VendorId { get { return 0x3633; } }
        public ushort ProductId { get; private set; }

        private SafeFileHandle _handle;
        private FileStream _stream;
        private bool _disposed;

        public DeepCoolDigitalDriver()
        {
            ProductId = 0x0001;
        }

        public bool IsConnected
        {
            get { return _handle != null && !_handle.IsInvalid && !_handle.IsClosed; }
        }

        public bool EnsureConnected()
        {
            if (IsConnected) return true;
            Close();

            // Tenta PID 0x0001 (AK Series), 0x0002 (LS Series) e 0x0003 (CH Series)
            ushort[] candidatePids = new ushort[] { 0x0001, 0x0002, 0x0003 };
            string devicePath = null;

            for (int i = 0; i < candidatePids.Length; i++)
            {
                devicePath = HidHelper.FindDevicePath(VendorId, candidatePids[i]);
                if (!string.IsNullOrEmpty(devicePath))
                {
                    ProductId = candidatePids[i];
                    break;
                }
            }

            if (string.IsNullOrEmpty(devicePath)) return false;

            try
            {
                IntPtr ptr = HidHelper.CreateFile(
                    devicePath,
                    HidHelper.GENERIC_READ | HidHelper.GENERIC_WRITE,
                    HidHelper.FILE_SHARE_READ | HidHelper.FILE_SHARE_WRITE,
                    IntPtr.Zero,
                    HidHelper.OPEN_EXISTING,
                    HidHelper.FILE_FLAG_OVERLAPPED,
                    IntPtr.Zero);

                if (ptr == HidHelper.INVALID_HANDLE_VALUE) return false;

                _handle = new SafeFileHandle(ptr, true);
                _stream = new FileStream(_handle, FileAccess.ReadWrite, 64, true);
                return true;
            }
            catch
            {
                Close();
                return false;
            }
        }

        public static byte[] BuildPacket(byte displayValue, HardwareMetrics metrics)
        {
            // Frame DeepCool Digital (64 bytes)
            byte[] buffer = new byte[64];
            buffer[0] = 0x00; // Report ID
            buffer[1] = 0xDA; // Header DeepCool
            buffer[2] = (byte)Math.Max(0, Math.Min(99, (int)displayValue)); // Valor numérico principal (0-99)
            buffer[3] = (byte)Math.Max(0, Math.Min(100, (int)Math.Round(metrics.CpuLoad))); // Barra de carga
            buffer[4] = 0x01; // Modo Celsius / Ativo
            return buffer;
        }

        public bool SendMetrics(byte displayValue, HardwareMetrics metrics)
        {
            if (!EnsureConnected() || _stream == null) return false;

            byte[] buffer = BuildPacket(displayValue, metrics);

            try
            {
                _stream.Write(buffer, 0, buffer.Length);
                _stream.Flush();
                return true;
            }
            catch
            {
                Close();
                return false;
            }
        }

        public void Close()
        {
            try
            {
                if (_stream != null) { _stream.Dispose(); _stream = null; }
            }
            catch { }

            try
            {
                if (_handle != null) { _handle.Dispose(); _handle = null; }
            }
            catch { }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Close();
                _disposed = true;
            }
        }
    }
}
