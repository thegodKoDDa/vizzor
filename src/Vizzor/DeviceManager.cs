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
            if (_supportedDrivers != null)
            {
                for (int i = 0; i < _supportedDrivers.Count; i++)
                {
                    var driver = _supportedDrivers[i];
                    if (driver.EnsureConnected())
                    {
                        _activeDevice = driver;
                        AppLog.Write("Cooler conectado: " + driver.DeviceName);
                        return true;
                    }
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

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_supportedDrivers != null)
                {
                    for (int i = 0; i < _supportedDrivers.Count; i++)
                    {
                        _supportedDrivers[i].Dispose();
                    }
                }
                _disposed = true;
            }
        }
    }
}
