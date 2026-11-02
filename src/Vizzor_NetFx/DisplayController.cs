using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Vizzor
{
    public sealed class DisplayController : IDisposable
    {
        private const ushort TargetVid = 0xAA88;
        private const ushort TargetPid = 0x8666;

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

            string devicePath = FindDevicePath(TargetVid, TargetPid);
            if (string.IsNullOrEmpty(devicePath)) return false;

            try
            {
                IntPtr ptr = NativeMethods.CreateFile(
                    devicePath,
                    NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE,
                    NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE,
                    IntPtr.Zero,
                    NativeMethods.OPEN_EXISTING,
                    NativeMethods.FILE_FLAG_OVERLAPPED,
                    IntPtr.Zero);

                if (ptr == NativeMethods.INVALID_HANDLE_VALUE) return false;

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

        public bool SendPacket(byte cpuTemp, byte gpuTemp, byte cpuLoad, byte ramLoad, byte mbTemp, byte gpuLoad)
        {
            if (!EnsureConnected() || _stream == null) return false;

            byte[] buffer = new byte[11];
            buffer[0] = 0x00;
            buffer[1] = cpuTemp;
            buffer[2] = gpuTemp;
            buffer[3] = cpuLoad;
            buffer[4] = ramLoad;
            buffer[5] = mbTemp;
            buffer[6] = gpuLoad;

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

        private static string FindDevicePath(ushort vid, ushort pid)
        {
            string vidStr = string.Format("vid_{0:x4}", vid);
            string pidStr = string.Format("pid_{0:x4}", pid);

            Guid hidGuid = Guid.Empty;
            NativeMethods.HidD_GetHidGuid(ref hidGuid);

            IntPtr devInfo = NativeMethods.SetupDiGetClassDevs(ref hidGuid, IntPtr.Zero, IntPtr.Zero, NativeMethods.DIGCF_DEVICEINTERFACE | NativeMethods.DIGCF_PRESENT);
            if (devInfo == NativeMethods.INVALID_HANDLE_VALUE) return null;

            try
            {
                NativeMethods.SP_DEVICE_INTERFACE_DATA ifData = new NativeMethods.SP_DEVICE_INTERFACE_DATA();
                ifData.cbSize = Marshal.SizeOf(typeof(NativeMethods.SP_DEVICE_INTERFACE_DATA));

                for (uint i = 0; i < 128; i++)
                {
                    if (!NativeMethods.SetupDiEnumDeviceInterfaces(devInfo, IntPtr.Zero, ref hidGuid, i, ref ifData))
                        break;

                    uint requiredSize = 0;
                    NativeMethods.SetupDiGetDeviceInterfaceDetail(devInfo, ref ifData, IntPtr.Zero, 0, out requiredSize, IntPtr.Zero);
                    if (requiredSize == 0) continue;

                    IntPtr detailBuffer = Marshal.AllocHGlobal((int)requiredSize);
                    try
                    {
                        Marshal.WriteInt32(detailBuffer, IntPtr.Size == 8 ? 8 : 4 + Marshal.SystemDefaultCharSize);

                        if (NativeMethods.SetupDiGetDeviceInterfaceDetail(devInfo, ref ifData, detailBuffer, requiredSize, out requiredSize, IntPtr.Zero))
                        {
                            IntPtr pDevicePath = new IntPtr(detailBuffer.ToInt64() + 4);
                            string devicePath = Marshal.PtrToStringAuto(pDevicePath);

                            if (!string.IsNullOrEmpty(devicePath))
                            {
                                string lower = devicePath.ToLowerInvariant();
                                if (lower.Contains(vidStr) && lower.Contains(pidStr))
                                {
                                    return devicePath;
                                }
                            }
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(detailBuffer);
                    }
                }
            }
            finally
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(devInfo);
            }

            return null;
        }

        private static class NativeMethods
        {
            public const uint GENERIC_READ = 0x80000000;
            public const uint GENERIC_WRITE = 0x40000000;
            public const uint FILE_SHARE_READ = 0x00000001;
            public const uint FILE_SHARE_WRITE = 0x00000002;
            public const uint OPEN_EXISTING = 3;
            public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
            public const int DIGCF_PRESENT = 0x00000002;
            public const int DIGCF_DEVICEINTERFACE = 0x00000010;

            public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

            [StructLayout(LayoutKind.Sequential)]
            public struct SP_DEVICE_INTERFACE_DATA
            {
                public int cbSize;
                public Guid interfaceClassGuid;
                public int flags;
                public IntPtr reserved;
            }

            [DllImport("hid.dll", SetLastError = true)]
            public static extern void HidD_GetHidGuid(ref Guid HidGuid);

            [DllImport("setupapi.dll", SetLastError = true)]
            public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr HwndParent, int Flags);

            [DllImport("setupapi.dll", SetLastError = true)]
            public static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref Guid InterfaceClassGuid, uint MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

            [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, uint DeviceInterfaceDetailDataSize, out uint RequiredSize, IntPtr DeviceInfoData);

            [DllImport("setupapi.dll", SetLastError = true)]
            public static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        }
    }
}
