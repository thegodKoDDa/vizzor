using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace Vizzor
{
    public sealed class LianLiDriver : IDisplayDevice
    {
        public string DeviceName { get { return "Lian Li Galahad II LCD / UNI FAN LCD"; } }
        public string Manufacturer { get { return "Lian Li Industrial"; } }
        public ushort VendorId { get { return 0x0CF2; } }
        public ushort ProductId { get; private set; }

        private SafeFileHandle _handle;
        private FileStream _stream;
        private bool _disposed;

        public LianLiDriver()
        {
            ProductId = 0xA102;
        }

        public bool IsConnected
        {
            get { return _handle != null && !_handle.IsInvalid && !_handle.IsClosed; }
        }

        public bool EnsureConnected()
        {
            if (IsConnected) return true;
            Close();

            ushort[] candidatePids = new ushort[] { 0xA102, 0xA100, 0xA101 };
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
            byte[] buffer = new byte[64];
            buffer[0] = 0x00;
            buffer[1] = 0xE0;
            buffer[2] = (byte)Math.Max(0, Math.Min(99, (int)displayValue));
            buffer[3] = (byte)Math.Max(0, Math.Min(100, (int)Math.Round(metrics.GpuTemp)));
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
