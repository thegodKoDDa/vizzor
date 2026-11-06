using System;
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Vizzor
{
    public sealed class RiseModeAuraIceDriver : IDisplayDevice
    {
        public string DeviceName { get { return "Rise Mode Aura Ice 360 (RM-WAIB-06-ARGB)"; } }
        public string Manufacturer { get { return "Rise Mode / 铭研科技 (Mingyan)"; } }
        public ushort VendorId { get { return 0xAA88; } }
        public ushort ProductId { get { return 0x8666; } }

        private SafeFileHandle _handle;
        private FileStream _stream;
        private bool _disposed;

        public bool IsConnected
        {
            get { return _handle != null && !_handle.IsInvalid && !_handle.IsClosed; }
        }

        public bool EnsureConnected()
        {
            if (IsConnected) return true;
            Close();

            string devicePath = HidHelper.FindDevicePath(VendorId, ProductId);
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

                if (ptr == HidHelper.INVALID_HANDLE_VALUE)
                {
                    int error = Marshal.GetLastWin32Error();
                    AppLog.Write("Falha ao abrir Rise Mode HID: " + new Win32Exception(error).Message);
                    return false;
                }

                _handle = new SafeFileHandle(ptr, true);
                _stream = new FileStream(_handle, FileAccess.ReadWrite, 64, true);
                return true;
            }
            catch (Exception ex)
            {
                AppLog.Write("Rise Mode HID: " + ex.Message);
                Close();
                return false;
            }
        }

        public static byte[] BuildPacket(byte displayValue, HardwareMetrics metrics)
        {
            // Frame padrao do Aura Ice (11 bytes, Report ID 0x00)
            byte[] buffer = new byte[11];
            buffer[0] = 0x00;
            buffer[1] = (byte)Math.Max(0, Math.Min(99, (int)displayValue));
            buffer[2] = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.GpuTemp)));
            buffer[3] = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.CpuLoad)));
            buffer[4] = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.RamLoad)));
            buffer[5] = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.MotherboardTemp)));
            buffer[6] = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.GpuLoad)));
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
            catch (Exception ex)
            {
                AppLog.Write("Rise Mode HID: " + ex.Message);
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
