using System;
using System.Collections.Generic;

namespace Vizzor
{
    public sealed class DeviceManager : IDisposable
    {
        private readonly List<IDisplayDevice> _supportedDrivers;
        private IDisplayDevice _activeDevice;
        private bool _disposed;
        private DateTime _nextDetection = DateTime.MinValue;

        public IDisplayDevice ActiveDevice
        {
            get { return _activeDevice; }
        }

        public IReadOnlyList<IDisplayDevice> SupportedDrivers
        {
            get { return _supportedDrivers; }
        }

        public DeviceManager()
        {
            _supportedDrivers = new List<IDisplayDevice>
            {
                new RiseModeAuraIceDriver(),
                new MingyanGenericDriver(),
                new DeepCoolDigitalDriver(),
                new NzxtKrakenDriver(),
                new LianLiDriver(),
                new CoolerMasterDriver()
            };

            AutoDetect();
        }

        public bool AutoDetect()
        {
            // 1. Se o dispositivo atual ainda estiver conectado, mantem
            if (_activeDevice != null && _activeDevice.IsConnected)
            {
                return true;
            }

            if (DateTime.UtcNow < _nextDetection) return false;
            _nextDetection = DateTime.UtcNow.AddSeconds(5);

            // 2. Itera sobre os drivers suportados e tenta conectar
            for (int i = 0; i < _supportedDrivers.LengthCount(); i++)
            {
                var driver = _supportedDrivers[i];
                if (driver.EnsureConnected())
                {
                    _activeDevice = driver;
                    AppLog.Write("Cooler conectado: " + driver.DeviceName);
                    return true;
                }
            }

            // Se nao encontrou nenhum dispositivo conectado, mantem nulo (vazio)
            _activeDevice = null;
            return false;
        }

        public bool IsDeviceConnected
        {
            get { return _activeDevice != null && _activeDevice.IsConnected; }
        }

        public bool SelectDriver(Type driverType)
        {
            for (int i = 0; i < _supportedDrivers.LengthCount(); i++)
            {
                if (_supportedDrivers[i].GetType() == driverType)
                {
                    _activeDevice = _supportedDrivers[i];
                    return _activeDevice.EnsureConnected();
                }
            }
            return false;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                for (int i = 0; i < _supportedDrivers.LengthCount(); i++)
                {
                    _supportedDrivers[i].Dispose();
                }
                _disposed = true;
            }
        }
    }

    internal static class ListExtensions
    {
        public static int LengthCount<T>(this IList<T> list)
        {
            return list != null ? list.Count : 0;
        }
    }
}
