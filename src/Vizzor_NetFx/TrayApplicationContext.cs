using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
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
        RamLoad,
        ClockMinutes,
        StealthOff
    }

    public sealed class TrayApplicationContext : ApplicationContext
    {
        private const string AppName = "Vizzor";
        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppRegistryKey = @"Software\Vizzor";

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private readonly NotifyIcon _notifyIcon;
        private readonly System.Windows.Forms.Timer _updateTimer;
        private readonly HardwareMonitor _hardwareMonitor;
        private readonly DeviceManager _deviceManager;
        private readonly DashboardForm _dashboardForm;

        private DisplayMode _currentMode = DisplayMode.CpuTemp;
        private int _updateInterval = 1000;
        private int _alternateSeconds = 5;
        private bool _highTempAlertEnabled = true;
        private bool _visualBlinkAlertEnabled = true;

        private int _alternateCounter = 0;
        private bool _alternateShowCpu = true;
        private int _logCycles = 0;
        private bool _blinkState = false;
        private DateTime _lastAlertTime = DateTime.MinValue;
        private IntPtr _lastHIcon = IntPtr.Zero;

        // Menus
        private ToolStripMenuItem _menuCpu;
        private ToolStripMenuItem _menuGpu;
        private ToolStripMenuItem _menuMax;
        private ToolStripMenuItem _menuAlt;
        private ToolStripMenuItem _menuCpuLoad;
        private ToolStripMenuItem _menuRamLoad;
        private ToolStripMenuItem _menuClock;
        private ToolStripMenuItem _menuStealth;

        private ToolStripMenuItem _menuInt500;
        private ToolStripMenuItem _menuInt1000;
        private ToolStripMenuItem _menuInt2000;

        private ToolStripMenuItem _menuAlt3;
        private ToolStripMenuItem _menuAlt5;
        private ToolStripMenuItem _menuAlt10;

        private ToolStripMenuItem _menuAlert;
        private ToolStripMenuItem _menuVisualBlink;
        private ToolStripMenuItem _menuAutostart;

        public TrayApplicationContext()
        {
            _hardwareMonitor = new HardwareMonitor();
            _deviceManager = new DeviceManager();

            LoadSettings();

            _dashboardForm = new DashboardForm(_hardwareMonitor, _deviceManager, SetMode, delegate { return _currentMode; });

            _notifyIcon = new NotifyIcon
            {
                Icon = CreateAppIcon(),
                Text = "Vizzor - Visor do Water Cooler",
                Visible = true,
                ContextMenuStrip = CreateContextMenu()
            };

            // Duplo clique abre o Dashboard
            _notifyIcon.DoubleClick += delegate
            {
                _dashboardForm.Show();
                _dashboardForm.BringToFront();
            };

            // Executa animacao de boot em thread de background antes do loop normal
            var bootThread = new Thread(RunBootAnimation);
            bootThread.IsBackground = true;
            bootThread.Start();

            _updateTimer = new System.Windows.Forms.Timer { Interval = _updateInterval };
            _updateTimer.Tick += OnUpdateTick;
            _updateTimer.Start();

            OnUpdateTick(this, EventArgs.Empty);
        }

        private void RunBootAnimation()
        {
            try
            {
                if (_deviceManager.ActiveDevice == null || !_deviceManager.ActiveDevice.EnsureConnected()) return;

                // Sequencia rapida de contagem estilo boot de corrida: 00 -> 11 -> 22 ... -> 99
                byte[] bootFrames = new byte[] { 0, 11, 22, 33, 44, 55, 66, 77, 88, 99 };
                var dummyMetrics = new HardwareMetrics();
                for (int i = 0; i < bootFrames.Length; i++)
                {
                    _deviceManager.ActiveDevice.SendMetrics(bootFrames[i], dummyMetrics);
                    Thread.Sleep(85);
                }
            }
            catch { }
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var menu = new ContextMenuStrip();

            var openDashItem = new ToolStripMenuItem("Abrir Painel de Controle...", null, delegate {
                _dashboardForm.Show();
                _dashboardForm.BringToFront();
            })
            {
                Font = new Font(Control.DefaultFont, FontStyle.Bold)
            };
            menu.Items.Add(openDashItem);

            var deviceItem = new ToolStripMenuItem("Cooler: " + _deviceManager.ActiveDevice.DeviceName) { Enabled = false };
            menu.Items.Add(deviceItem);
            menu.Items.Add(new ToolStripSeparator());

            // 1. Modos de Exibicao
            _menuCpu = new ToolStripMenuItem("Temperatura da CPU", null, delegate { SetMode(DisplayMode.CpuTemp); });
            _menuGpu = new ToolStripMenuItem("Temperatura da GPU", null, delegate { SetMode(DisplayMode.GpuTemp); });
            _menuMax = new ToolStripMenuItem("O Mais Quente (CPU/GPU)", null, delegate { SetMode(DisplayMode.HighestTemp); });
            _menuAlt = new ToolStripMenuItem("Alternar (CPU e GPU)", null, delegate { SetMode(DisplayMode.Alternating); });
            _menuClock = new ToolStripMenuItem("Modo Relógio (Minutos)", null, delegate { SetMode(DisplayMode.ClockMinutes); });
            _menuCpuLoad = new ToolStripMenuItem("Uso da CPU (%)", null, delegate { SetMode(DisplayMode.CpuLoad); });
            _menuRamLoad = new ToolStripMenuItem("Uso de Memória RAM (%)", null, delegate { SetMode(DisplayMode.RamLoad); });
            _menuStealth = new ToolStripMenuItem("Modo Stealth (Apagar Visor)", null, delegate { SetMode(DisplayMode.StealthOff); });

            var displayMenu = new ToolStripMenuItem("Modo de Exibição");
            displayMenu.DropDownItems.AddRange(new ToolStripItem[] {
                _menuCpu, _menuGpu, _menuMax, _menuAlt, _menuClock,
                new ToolStripSeparator(),
                _menuCpuLoad, _menuRamLoad,
                new ToolStripSeparator(),
                _menuStealth
            });
            menu.Items.Add(displayMenu);

            // 2. Intervalo de Atualizacao
            _menuInt500 = new ToolStripMenuItem("Rápido (500 ms)", null, delegate { SetInterval(500); });
            _menuInt1000 = new ToolStripMenuItem("Normal (1 segundo)", null, delegate { SetInterval(1000); });
            _menuInt2000 = new ToolStripMenuItem("Econômico (2 segundos)", null, delegate { SetInterval(2000); });

            var intervalMenu = new ToolStripMenuItem("Velocidade de Atualização");
            intervalMenu.DropDownItems.AddRange(new ToolStripItem[] { _menuInt500, _menuInt1000, _menuInt2000 });
            menu.Items.Add(intervalMenu);

            // 3. Tempo de Alternancia
            _menuAlt3 = new ToolStripMenuItem("A cada 3 segundos", null, delegate { SetAlternateSeconds(3); });
            _menuAlt5 = new ToolStripMenuItem("A cada 5 segundos", null, delegate { SetAlternateSeconds(5); });
            _menuAlt10 = new ToolStripMenuItem("A cada 10 segundos", null, delegate { SetAlternateSeconds(10); });

            var altMenu = new ToolStripMenuItem("Tempo de Alternância");
            altMenu.DropDownItems.AddRange(new ToolStripItem[] { _menuAlt3, _menuAlt5, _menuAlt10 });
            menu.Items.Add(altMenu);

            menu.Items.Add(new ToolStripSeparator());

            // 4. Efeitos e Alertas
            _menuVisualBlink = new ToolStripMenuItem("Efeito: Piscar Visor em Alta Temp (> 85°C)", null, delegate {
                _visualBlinkAlertEnabled = !_visualBlinkAlertEnabled;
                _menuVisualBlink.Checked = _visualBlinkAlertEnabled;
                SaveSettings();
            });
            _menuVisualBlink.Checked = _visualBlinkAlertEnabled;
            menu.Items.Add(_menuVisualBlink);

            _menuAlert = new ToolStripMenuItem("Notificação no Windows (> 85°C)", null, delegate {
                _highTempAlertEnabled = !_highTempAlertEnabled;
                _menuAlert.Checked = _highTempAlertEnabled;
                SaveSettings();
            });
            _menuAlert.Checked = _highTempAlertEnabled;
            menu.Items.Add(_menuAlert);

            _menuAutostart = new ToolStripMenuItem("Iniciar com o Windows", null, OnToggleAutostart);
            _menuAutostart.Checked = IsAutostartEnabled();
            menu.Items.Add(_menuAutostart);

            menu.Items.Add(new ToolStripSeparator());

            var cleanOldItem = new ToolStripMenuItem("Desativar Software Antigo da Rise Mode", null, OnDisableOldSoftware);
            menu.Items.Add(cleanOldItem);

            menu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem("Sair do Vizzor", null, OnExit);
            menu.Items.Add(exitItem);

            UpdateMenuChecks();
            return menu;
        }

        public void SetMode(DisplayMode mode)
        {
            _currentMode = mode;
            _alternateCounter = 0;
            _alternateShowCpu = true;
            UpdateMenuChecks();
            SaveSettings();
        }

        private void SetInterval(int ms)
        {
            _updateInterval = ms;
            _updateTimer.Interval = _updateInterval;
            UpdateMenuChecks();
            SaveSettings();
        }

        private void SetAlternateSeconds(int sec)
        {
            _alternateSeconds = sec;
            _alternateCounter = 0;
            UpdateMenuChecks();
            SaveSettings();
        }

        private void UpdateMenuChecks()
        {
            _menuCpu.Checked = _currentMode == DisplayMode.CpuTemp;
            _menuGpu.Checked = _currentMode == DisplayMode.GpuTemp;
            _menuMax.Checked = _currentMode == DisplayMode.HighestTemp;
            _menuAlt.Checked = _currentMode == DisplayMode.Alternating;
            _menuClock.Checked = _currentMode == DisplayMode.ClockMinutes;
            _menuCpuLoad.Checked = _currentMode == DisplayMode.CpuLoad;
            _menuRamLoad.Checked = _currentMode == DisplayMode.RamLoad;
            _menuStealth.Checked = _currentMode == DisplayMode.StealthOff;

            _menuInt500.Checked = _updateInterval == 500;
            _menuInt1000.Checked = _updateInterval == 1000;
            _menuInt2000.Checked = _updateInterval == 2000;

            _menuAlt3.Checked = _alternateSeconds == 3;
            _menuAlt5.Checked = _alternateSeconds == 5;
            _menuAlt10.Checked = _alternateSeconds == 10;
        }

        private void OnUpdateTick(object sender, EventArgs e)
        {
            HardwareMetrics metrics = _hardwareMonitor.GetMetrics();

            // Verificacao de Alerta de Notificacao no Windows
            if (_highTempAlertEnabled && (DateTime.Now - _lastAlertTime).TotalSeconds > 60)
            {
                if (metrics.CpuTemp >= 85 || metrics.GpuTemp >= 85)
                {
                    _lastAlertTime = DateTime.Now;
                    string component = metrics.CpuTemp >= 85 ? "CPU" : "GPU";
                    float temp = Math.Max(metrics.CpuTemp, metrics.GpuTemp);
                    _notifyIcon.ShowBalloonTip(3000, "Vizzor - Alerta de Temperatura", string.Format("Atenção: {0} atingiu {1:F0}°C!", component, temp), ToolTipIcon.Warning);
                }
            }

            // Modo Stealth: simplesmente nao envia pacotes para o visor apagar por watchdog
            if (_currentMode == DisplayMode.StealthOff)
            {
                UpdateDynamicTrayIcon(0, false);
                _notifyIcon.Text = string.Format("Vizzor (Visor Apagado)\nCPU: {0:F0}°C | GPU: {1:F0}°C", metrics.CpuTemp, metrics.GpuTemp);
                _dashboardForm.UpdateTelemetry(metrics, 0, _currentMode);
                return;
            }

            byte displayValue = 0;
            bool isTempDisplay = true;

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
                    _alternateCounter += (_updateInterval / 1000 > 0 ? _updateInterval / 1000 : 1);
                    if (_alternateCounter >= _alternateSeconds)
                    {
                        _alternateCounter = 0;
                        _alternateShowCpu = !_alternateShowCpu;
                    }
                    float val = _alternateShowCpu ? metrics.CpuTemp : metrics.GpuTemp;
                    displayValue = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(val)));
                    break;
                case DisplayMode.ClockMinutes:
                    displayValue = (byte)DateTime.Now.Minute;
                    isTempDisplay = false;
                    break;
                case DisplayMode.CpuLoad:
                    displayValue = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.CpuLoad)));
                    isTempDisplay = false;
                    break;
                case DisplayMode.RamLoad:
                    displayValue = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.RamLoad)));
                    isTempDisplay = false;
                    break;
            }

            // Efeito Alerta Visual no Gabinete: se temp >= 85°C e o efeito estiver ativo, pisca o visor físico
            byte sentDisplayValue = displayValue;
            if (_visualBlinkAlertEnabled && (metrics.CpuTemp >= 85 || metrics.GpuTemp >= 85))
            {
                _blinkState = !_blinkState;
                if (_blinkState)
                {
                    sentDisplayValue = 99; // Alerta visual de pico no display
                }
            }

            byte cpuTemp = sentDisplayValue;
            byte gpuTemp = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.GpuTemp)));
            byte cpuLoad = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.CpuLoad)));
            byte ramLoad = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.RamLoad)));
            byte mbTemp = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.MotherboardTemp)));
            byte gpuLoad = (byte)Math.Max(0, Math.Min(99, (int)Math.Round(metrics.GpuLoad)));

            bool sent = _deviceManager.ActiveDevice != null && _deviceManager.ActiveDevice.SendMetrics(sentDisplayValue, metrics);

            // Atualiza o Ícone Dinâmico da Bandeja com o valor ao vivo
            UpdateDynamicTrayIcon(displayValue, isTempDisplay);

            // Atualiza a janela visual do Dashboard (se estiver visivel)
            _dashboardForm.UpdateTelemetry(metrics, displayValue, _currentMode);

            if (_logCycles < 5)
            {
                _logCycles++;
                try { File.AppendAllText(@"C:\Vizzor\vizzor.log", string.Format("[Tick {0}] CPU={1:F1}C, GPU={2:F1}C, DisplayVal={3}, Sent={4}\n", _logCycles, metrics.CpuTemp, metrics.GpuTemp, displayValue, sent)); } catch { }
            }

            string status = sent ? "Conectado" : "Buscando visor...";
            string tooltip = string.Format("Vizzor ({0})\nCPU: {1:F0}°C ({2:F0}%)\nGPU: {3:F0}°C ({4:F0}%)\nRAM: {5:F0}%\nVisor: {6}", status, metrics.CpuTemp, metrics.CpuLoad, metrics.GpuTemp, metrics.GpuLoad, metrics.RamLoad, displayValue);

            if (tooltip.Length >= 64)
            {
                tooltip = string.Format("Vizzor: {0} | CPU: {1:F0}°C | GPU: {2:F0}°C", displayValue, metrics.CpuTemp, metrics.GpuTemp);
                if (tooltip.Length >= 64) tooltip = tooltip.Substring(0, 63);
            }

            _notifyIcon.Text = tooltip;
        }

        private void UpdateDynamicTrayIcon(int value, bool isTemp)
        {
            try
            {
                using (var bmp = new Bitmap(16, 16))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.Transparent);

                        Color textColor;
                        if (isTemp)
                        {
                            if (value >= 80) textColor = Color.FromArgb(255, 60, 60);       // Vermelho
                            else if (value >= 65) textColor = Color.FromArgb(255, 175, 40);   // Laranja
                            else textColor = Color.FromArgb(0, 220, 255);                     // Ciano
                        }
                        else
                        {
                            textColor = Color.FromArgb(80, 230, 120); // Verde para carga/RAM
                        }

                        string text = value > 99 ? "99" : (value == 0 ? "V" : value.ToString());
                        using (var font = new Font("Segoe UI", 9.5f, FontStyle.Bold, GraphicsUnit.Pixel))
                        using (var brush = new SolidBrush(textColor))
                        {
                            var size = g.MeasureString(text, font);
                            float x = (16f - size.Width) / 2f + 0.5f;
                            float y = (16f - size.Height) / 2f + 1f;
                            g.DrawString(text, font, brush, new PointF(x, y));
                        }
                    }

                    IntPtr hIcon = bmp.GetHicon();
                    _notifyIcon.Icon = Icon.FromHandle(hIcon);

                    if (_lastHIcon != IntPtr.Zero)
                    {
                        DestroyIcon(_lastHIcon);
                    }
                    _lastHIcon = hIcon;
                }
            }
            catch { }
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

        private void OnDisableOldSoftware(object sender, EventArgs e)
        {
            if (MessageBox.Show("Deseja desativar o servico e a inicializacao automatica do software antigo da Rise Mode?", "Vizzor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c net stop tifSer 2>nul & sc config tifSer start=disabled 2>nul & taskkill /F /IM \"SendTemp.exe\" 2>nul & taskkill /F /IM \"Rise Mode Temp CPU Driver R2.2.exe\" 2>nul",
                        Verb = "runas",
                        CreateNoWindow = true,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                    });
                    MessageBox.Show("Software antigo desativado com sucesso!", "Vizzor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Falha ao desativar: " + ex.Message, "Vizzor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
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
                        object modeVal = key.GetValue("DisplayMode");
                        if (modeVal is int) _currentMode = (DisplayMode)(int)modeVal;

                        object intVal = key.GetValue("UpdateInterval");
                        if (intVal is int) _updateInterval = (int)intVal;

                        object altVal = key.GetValue("AlternateSeconds");
                        if (altVal is int) _alternateSeconds = (int)altVal;

                        object alertVal = key.GetValue("HighTempAlert");
                        if (alertVal is int) _highTempAlertEnabled = ((int)alertVal) == 1;

                        object blinkVal = key.GetValue("VisualBlinkAlert");
                        if (blinkVal is int) _visualBlinkAlertEnabled = ((int)blinkVal) == 1;
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
                        key.SetValue("UpdateInterval", _updateInterval);
                        key.SetValue("AlternateSeconds", _alternateSeconds);
                        key.SetValue("HighTempAlert", _highTempAlertEnabled ? 1 : 0);
                        key.SetValue("VisualBlinkAlert", _visualBlinkAlertEnabled ? 1 : 0);
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
            _deviceManager.Dispose();
            _hardwareMonitor.Dispose();
            if (_lastHIcon != IntPtr.Zero)
            {
                DestroyIcon(_lastHIcon);
                _lastHIcon = IntPtr.Zero;
            }
            _notifyIcon.Dispose();
            ExitThread();
        }
    }
}
