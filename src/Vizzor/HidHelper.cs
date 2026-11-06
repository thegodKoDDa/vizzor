using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Vizzor
{
    public static class HidHelper
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

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetManufacturerString(IntPtr HidDeviceObject, IntPtr Buffer, uint BufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetProductString(IntPtr HidDeviceObject, IntPtr Buffer, uint BufferLength);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        public sealed class UsbDeviceInfo
        {
            public string DevicePath { get; set; }
            public string Vid { get; set; }
            public string Pid { get; set; }
            public string Manufacturer { get; set; }
            public string Product { get; set; }
            public bool IsSupported { get; set; }
        }

        private static void TryReadHidStrings(string devicePath, out string manufacturer, out string product)
        {
            manufacturer = null;
            product = null;

            IntPtr ptr = CreateFile(
                devicePath,
                0,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero,
                OPEN_EXISTING,
                0,
                IntPtr.Zero);

            if (ptr == INVALID_HANDLE_VALUE) return;

            using (var handle = new SafeFileHandle(ptr, true))
            {
                const int bufferBytes = 256;
                IntPtr strBuffer = Marshal.AllocHGlobal(bufferBytes);
                try
                {
                    if (HidD_GetManufacturerString(handle.DangerousGetHandle(), strBuffer, bufferBytes))
                    {
                        string m = Marshal.PtrToStringUni(strBuffer);
                        if (!string.IsNullOrWhiteSpace(m)) manufacturer = m.Trim();
                    }

                    if (HidD_GetProductString(handle.DangerousGetHandle(), strBuffer, bufferBytes))
                    {
                        string p = Marshal.PtrToStringUni(strBuffer);
                        if (!string.IsNullOrWhiteSpace(p)) product = p.Trim();
                    }
                }
                catch { }
                finally
                {
                    Marshal.FreeHGlobal(strBuffer);
                }
            }
        }

        public static List<UsbDeviceInfo> ScanAllHidDevices()
        {
            var list = new List<UsbDeviceInfo>();
            Guid hidGuid = Guid.Empty;
            HidD_GetHidGuid(ref hidGuid);

            IntPtr devInfo = SetupDiGetClassDevs(ref hidGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_DEVICEINTERFACE | DIGCF_PRESENT);
            if (devInfo == INVALID_HANDLE_VALUE) return list;

            try
            {
                SP_DEVICE_INTERFACE_DATA ifData = new SP_DEVICE_INTERFACE_DATA();
                ifData.cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DATA));

                for (uint i = 0; i < 4096; i++)
                {
                    if (!SetupDiEnumDeviceInterfaces(devInfo, IntPtr.Zero, ref hidGuid, i, ref ifData))
                        break;

                    uint requiredSize = 0;
                    SetupDiGetDeviceInterfaceDetail(devInfo, ref ifData, IntPtr.Zero, 0, out requiredSize, IntPtr.Zero);
                    if (requiredSize == 0) continue;

                    IntPtr detailBuffer = Marshal.AllocHGlobal((int)requiredSize);
                    try
                    {
                        Marshal.WriteInt32(detailBuffer, IntPtr.Size == 8 ? 8 : 4 + Marshal.SystemDefaultCharSize);

                        if (SetupDiGetDeviceInterfaceDetail(devInfo, ref ifData, detailBuffer, requiredSize, out requiredSize, IntPtr.Zero))
                        {
                            IntPtr pDevicePath = new IntPtr(detailBuffer.ToInt64() + 4);
                            string path = Marshal.PtrToStringAuto(pDevicePath);

                            if (!string.IsNullOrEmpty(path))
                            {
                                string lower = path.ToLowerInvariant();
                                int vidIdx = lower.IndexOf("vid_");
                                int pidIdx = lower.IndexOf("pid_");

                                if (vidIdx >= 0 && vidIdx + 8 <= lower.Length &&
                                    pidIdx >= 0 && pidIdx + 8 <= lower.Length)
                                {
                                    string vid = lower.Substring(vidIdx + 4, 4);
                                    string pid = lower.Substring(pidIdx + 4, 4);

                                    string rawMfg, rawProd;
                                    TryReadHidStrings(path, out rawMfg, out rawProd);

                                    var info = new UsbDeviceInfo
                                    {
                                        DevicePath = path,
                                        Vid = "0x" + vid.ToUpperInvariant(),
                                        Pid = "0x" + pid.ToUpperInvariant(),
                                        Manufacturer = !string.IsNullOrEmpty(rawMfg) ? rawMfg : Strings.UnknownManufacturer,
                                        Product = !string.IsNullOrEmpty(rawProd) ? rawProd : Strings.GenericHidProduct,
                                        IsSupported = (vid == "aa88" || vid == "3633" || vid == "1e71" || vid == "0cf2" || vid == "2516")
                                    };

                                    if (vid == "aa88")
                                    {
                                        if (string.IsNullOrEmpty(rawProd)) info.Product = "Rise Mode Aura Ice / Mingyan Display";
                                        if (string.IsNullOrEmpty(rawMfg)) info.Manufacturer = "Mingyan Tech";
                                    }
                                    else if (vid == "3633")
                                    {
                                        if (string.IsNullOrEmpty(rawProd)) info.Product = "DeepCool Digital Display";
                                        if (string.IsNullOrEmpty(rawMfg)) info.Manufacturer = "DeepCool";
                                    }
                                    else if (vid == "1e71")
                                    {
                                        if (string.IsNullOrEmpty(rawProd)) info.Product = "NZXT Kraken Display";
                                        if (string.IsNullOrEmpty(rawMfg)) info.Manufacturer = "NZXT";
                                    }
                                    else if (vid == "0cf2")
                                    {
                                        if (string.IsNullOrEmpty(rawProd)) info.Product = "Lian Li Display";
                                        if (string.IsNullOrEmpty(rawMfg)) info.Manufacturer = "Lian Li";
                                    }
                                    else if (vid == "2516")
                                    {
                                        if (string.IsNullOrEmpty(rawProd)) info.Product = "Cooler Master Display";
                                        if (string.IsNullOrEmpty(rawMfg)) info.Manufacturer = "Cooler Master";
                                    }

                                    list.Add(info);
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
                SetupDiDestroyDeviceInfoList(devInfo);
            }

            return list;
        }

        public static string FindDevicePath(ushort vid, ushort pid)
        {
            string vidStr = string.Format("vid_{0:x4}", vid);
            string pidStr = string.Format("pid_{0:x4}", pid);

            Guid hidGuid = Guid.Empty;
            HidD_GetHidGuid(ref hidGuid);

            IntPtr devInfo = SetupDiGetClassDevs(ref hidGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_DEVICEINTERFACE | DIGCF_PRESENT);
            if (devInfo == INVALID_HANDLE_VALUE) return null;

            try
            {
                SP_DEVICE_INTERFACE_DATA ifData = new SP_DEVICE_INTERFACE_DATA();
                ifData.cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DATA));

                for (uint i = 0; i < 4096; i++)
                {
                    if (!SetupDiEnumDeviceInterfaces(devInfo, IntPtr.Zero, ref hidGuid, i, ref ifData))
                        break;

                    uint requiredSize = 0;
                    SetupDiGetDeviceInterfaceDetail(devInfo, ref ifData, IntPtr.Zero, 0, out requiredSize, IntPtr.Zero);
                    if (requiredSize == 0) continue;

                    IntPtr detailBuffer = Marshal.AllocHGlobal((int)requiredSize);
                    try
                    {
                        Marshal.WriteInt32(detailBuffer, IntPtr.Size == 8 ? 8 : 4 + Marshal.SystemDefaultCharSize);

                        if (SetupDiGetDeviceInterfaceDetail(devInfo, ref ifData, detailBuffer, requiredSize, out requiredSize, IntPtr.Zero))
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
                SetupDiDestroyDeviceInfoList(devInfo);
            }

            return null;
        }
    }
}
