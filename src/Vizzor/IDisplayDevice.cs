using System;

namespace Vizzor
{
    public interface IDisplayDevice : IDisposable
    {
        string DeviceName { get; }
        string Manufacturer { get; }
        ushort VendorId { get; }
        ushort ProductId { get; }
        bool IsConnected { get; }

        bool EnsureConnected();
        bool SendMetrics(byte displayValue, HardwareMetrics metrics);
        void Close();
    }
}
