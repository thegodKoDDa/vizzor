using System;
using System.Collections.Generic;

namespace Vizzor
{
    public sealed class DeviceManager : IDisposable
    {
        private readonly List<IDisplayDevice> _supportedDrivers;
        private IDisplayDevice _activeDevice;
        private bool _disposed;

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
                new DeepCoolDigitalDriver()
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

            // 2. Itera sobre os drivers suportados e tenta conectar
            for (int i = 0; i < _supportedDrivers.LengthCount(); i++)
            {
                var driver = _supportedDrivers[i];
                if (driver.EnsureConnected())
                {
                    _activeDevice = driver;
                    return true;
                }
            }

            // Fallback: seleciona o Rise Mode padrao
            _activeDevice = _supportedDrivers[0];
            return false;
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
