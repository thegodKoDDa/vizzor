using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Vizzor
{
    public enum DisplayMode
    {
        CpuTemp,
        GpuTemp,
        HighestTemp,
        Alternating,
        CpuLoad,
        RamLoad
    }

    public sealed class TrayApplicationContext : ApplicationContext
    {
        private const string AppName = "Vizzor";
        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppRegistryKey = @"Software\Vizzor";

        private readonly NotifyIcon _notifyIcon;
        private readonly Timer _updateTimer;
        private readonly HardwareMonitor _hardwareMonitor;
        private readonly DisplayController _displayController;

        private DisplayMode _currentMode = DisplayMode.CpuTemp;
        private int _alternateCounter = 0;
        private bool _alternateShowCpu = true;
        private int _logCycles = 0;

        private ToolStripMenuItem _menuCpu;
        private ToolStripMenuItem _menuGpu;
        private ToolStripMenuItem _menuMax;
        private ToolStripMenuItem _menuAlt;
        private ToolStripMenuItem _menuCpuLoad;
        private ToolStripMenuItem _menuRamLoad;
        private ToolStripMenuItem _menuAutostart;

        public TrayApplicationContext()
        {
            _hardwareMonitor = new HardwareMonitor();
            _displayController = new DisplayController();

            LoadSettings();

            _notifyIcon = new NotifyIcon
            {
                Icon = CreateAppIcon(),
                Text = "Vizzor - Visor do Water Cooler",
                Visible = true,
                ContextMenuStrip = CreateContextMenu()
            };

            _updateTimer = new Timer { Interval = 1000 };
            _updateTimer.Tick += OnUpdateTick;
            _updateTimer.Start();

            OnUpdateTick(this, EventArgs.Empty);
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var menu = new ContextMenuStrip();

            var titleItem = new ToolStripMenuItem("Vizzor (Rise Mode Aura Ice)")
            {
                Enabled = false,
                Font = new Font(Control.DefaultFont, FontStyle.Bold)
            };
            menu.Items.Add(titleItem);
            menu.Items.Add(new ToolStripSeparator());

            _menuCpu = new ToolStripMenuItem("Exibir: Temperatura da CPU", null, delegate { SetMode(DisplayMode.CpuTemp); });
            _menuGpu = new ToolStripMenuItem("Exibir: Temperatura da GPU", null, delegate { SetMode(DisplayMode.GpuTemp); });
            _menuMax = new ToolStripMenuItem("Exibir: O Mais Quente (CPU/GPU)", null, delegate { SetMode(DisplayMode.HighestTemp); });
            _menuAlt = new ToolStripMenuItem("Exibir: Alternar a cada 5s (CPU / GPU)", null, delegate { SetMode(DisplayMode.Alternating); });
            _menuCpuLoad = new ToolStripMenuItem("Exibir: Uso da CPU (%)", null, delegate { SetMode(DisplayMode.CpuLoad); });
            _menuRamLoad = new ToolStripMenuItem("Exibir: Uso de Memória RAM (%)", null, delegate { SetMode(DisplayMode.RamLoad); });

            menu.Items.Add(_menuCpu);
            menu.Items.Add(_menuGpu);
            menu.Items.Add(_menuMax);
            menu.Items.Add(_menuAlt);
            menu.Items.Add(_menuCpuLoad);
            menu.Items.Add(_menuRamLoad);

            menu.Items.Add(new ToolStripSeparator());

            _menuAutostart = new ToolStripMenuItem("Iniciar com o Windows", null, OnToggleAutostart);
            _menuAutostart.Checked = IsAutostartEnabled();
            menu.Items.Add(_menuAutostart);

            menu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem("Sair", null, OnExit);
            menu.Items.Add(exitItem);

            UpdateMenuChecks();
            return menu;
        }

        private void SetMode(DisplayMode mode)
        {
            _currentMode = mode;
            _alternateCounter = 0;
            _alternateShowCpu = true;
            UpdateMenuChecks();
            SaveSettings();
        }

        private void UpdateMenuChecks()
        {
            _menuCpu.Checked = _currentMode == DisplayMode.CpuTemp;
            _menuGpu.Checked = _currentMode == DisplayMode.GpuTemp;
            _menuMax.Checked = _currentMode == DisplayMode.HighestTemp;
            _menuAlt.Checked = _currentMode == DisplayMode.Alternating;
            _menuCpuLoad.Checked = _currentMode == DisplayMode.CpuLoad;
            _menuRamLoad.Checked = _currentMode == DisplayMode.RamLoad;
        }

        private void OnUpdateTick(object sender, EventArgs e)
        {
            HardwareMetrics metrics = _hardwareMonitor.GetMetrics();
            byte displayValue = 0;

            switch (_currentMode)
            {
                case DisplayMode.CpuTemp:
                    displayValue = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.CpuTemp)));
                    break;
                case DisplayMode.GpuTemp:
                    displayValue = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.GpuTemp)));
                    break;
                case DisplayMode.HighestTemp:
                    float highest = Math.Max(metrics.CpuTemp, metrics.GpuTemp);
                    displayValue = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(highest)));
                    break;
                case DisplayMode.Alternating:
                    _alternateCounter++;
                    if (_alternateCounter >= 5)
                    {
                        _alternateCounter = 0;
                        _alternateShowCpu = !_alternateShowCpu;
                    }
                    float val = _alternateShowCpu ? metrics.CpuTemp : metrics.GpuTemp;
                    displayValue = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(val)));
                    break;
                case DisplayMode.CpuLoad:
                    displayValue = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.CpuLoad)));
                    break;
                case DisplayMode.RamLoad:
                    displayValue = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.RamLoad)));
                    break;
            }

            byte cpuTemp = displayValue;
            byte gpuTemp = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.GpuTemp)));
            byte cpuLoad = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.CpuLoad)));
            byte ramLoad = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.RamLoad)));
            byte mbTemp = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.MotherboardTemp)));
            byte gpuLoad = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.GpuLoad)));

            bool sent = _displayController.SendPacket(cpuTemp, gpuTemp, cpuLoad, ramLoad, mbTemp, gpuLoad);

            if (_logCycles < 10)
            {
                _logCycles++;
                try { File.AppendAllText(@"C:\Vizzor\vizzor.log", string.Format("[Tick {0}] CPU={1:F1}C, GPU={2:F1}C, DisplayVal={3}, Sent={4}\n", _logCycles, metrics.CpuTemp, metrics.GpuTemp, displayValue, sent)); } catch { }
            }

            string status = sent ? "Conectado" : "Buscando visor...";
            string tooltip = string.Format("Vizzor ({0})\nCPU: {1:F0}°C ({2:F0}%)\nGPU: {3:F0}°C ({4:F0}%)\nVisor: {5}", status, metrics.CpuTemp, metrics.CpuLoad, metrics.GpuTemp, metrics.GpuLoad, displayValue);

            if (tooltip.Length >= 64)
            {
                tooltip = string.Format("Vizzor: {0} | CPU: {1:F0}°C | GPU: {2:F0}°C", displayValue, metrics.CpuTemp, metrics.GpuTemp);
                if (tooltip.Length >= 64) tooltip = tooltip.Substring(0, 63);
            }

            _notifyIcon.Text = tooltip;
        }

        private void OnToggleAutostart(object sender, EventArgs e)
        {
            bool enable = !_menuAutostart.Checked;
            SetAutostart(enable);
            _menuAutostart.Checked = enable;
        }

        private static bool IsAutostartEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false))
                {
                    return key != null && key.GetValue(AppName) != null;
                }
            }
            catch { return false; }
        }

        private static void SetAutostart(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true))
                {
                    if (key != null)
                    {
                        if (enable)
                        {
                            string exePath = Application.ExecutablePath;
                            key.SetValue(AppName, "\"" + exePath + "\"");
                        }
                        else
                        {
                            key.DeleteValue(AppName, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Falha ao configurar inicializacao automatica: " + ex.Message, "Vizzor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadSettings()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(AppRegistryKey, false))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("DisplayMode");
                        if (val is int)
                        {
                            _currentMode = (DisplayMode)(int)val;
                        }
                    }
                }
            }
            catch { }
        }

        private void SaveSettings()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(AppRegistryKey))
                {
                    if (key != null)
                    {
                        key.SetValue("DisplayMode", (int)_currentMode);
                    }
                }
            }
            catch { }
        }

        private static Icon CreateAppIcon()
        {
            using (var bmp = new Bitmap(16, 16))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    using (var brush = new SolidBrush(Color.FromArgb(0, 180, 255)))
                    using (var font = new Font("Segoe UI", 10, FontStyle.Bold, GraphicsUnit.Pixel))
                    {
                        g.DrawString("V", font, brush, new PointF(3, 1));
                    }
                }
                IntPtr hIcon = bmp.GetHicon();
                return Icon.FromHandle(hIcon);
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            _updateTimer.Stop();
            _notifyIcon.Visible = false;
            _displayController.Dispose();
            _hardwareMonitor.Dispose();
            _notifyIcon.Dispose();
            ExitThread();
        }
    }
}
