using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Vizzor;

public sealed class TrayApplicationContext : ApplicationContext
{
    private const string AppName = "Vizzor";
    private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppRegistryKey = @"Software\Vizzor";

    private readonly NotifyIcon _notifyIcon;
    private readonly System.Windows.Forms.Timer _updateTimer;
    private readonly HardwareMonitor _hardwareMonitor;
    private readonly DisplayController _displayController;

    private DisplayMode _currentMode = DisplayMode.CpuTemp;
    private int _alternateCounter = 0;
    private bool _alternateShowCpu = true;
    private int _logCycles = 0;

    private ToolStripMenuItem _menuCpu = null!;
    private ToolStripMenuItem _menuGpu = null!;
    private ToolStripMenuItem _menuMax = null!;
    private ToolStripMenuItem _menuAlt = null!;
    private ToolStripMenuItem _menuCpuLoad = null!;
    private ToolStripMenuItem _menuRamLoad = null!;
    private ToolStripMenuItem _menuAutostart = null!;

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

        _updateTimer = new System.Windows.Forms.Timer
        {
            Interval = 1000 // 1 segundo
        };
        _updateTimer.Tick += OnUpdateTick;
        _updateTimer.Start();

        // Primeira execução imediata
        OnUpdateTick(this, EventArgs.Empty);
    }

    private ContextMenuStrip CreateContextMenu()
    {
        var menu = new ContextMenuStrip();

        var titleItem = new ToolStripMenuItem("Vizzor v1.0 (Rise Mode Aura Ice)")
        {
            Enabled = false,
            Font = new Font(Control.DefaultFont, FontStyle.Bold)
        };
        menu.Items.Add(titleItem);
        menu.Items.Add(new ToolStripSeparator());

        _menuCpu = new ToolStripMenuItem("Exibir: Temperatura da CPU", null, (s, e) => SetMode(DisplayMode.CpuTemp));
        _menuGpu = new ToolStripMenuItem("Exibir: Temperatura da GPU", null, (s, e) => SetMode(DisplayMode.GpuTemp));
        _menuMax = new ToolStripMenuItem("Exibir: O Mais Quente (CPU/GPU)", null, (s, e) => SetMode(DisplayMode.HighestTemp));
        _menuAlt = new ToolStripMenuItem("Exibir: Alternar a cada 5s (CPU / GPU)", null, (s, e) => SetMode(DisplayMode.Alternating));
        _menuCpuLoad = new ToolStripMenuItem("Exibir: Uso da CPU (%)", null, (s, e) => SetMode(DisplayMode.CpuLoad));
        _menuRamLoad = new ToolStripMenuItem("Exibir: Uso de Memória RAM (%)", null, (s, e) => SetMode(DisplayMode.RamLoad));

        menu.Items.Add(_menuCpu);
        menu.Items.Add(_menuGpu);
        menu.Items.Add(_menuMax);
        menu.Items.Add(_menuAlt);
        menu.Items.Add(_menuCpuLoad);
        menu.Items.Add(_menuRamLoad);

        menu.Items.Add(new ToolStripSeparator());

        _menuAutostart = new ToolStripMenuItem("Iniciar com o Windows", null, OnToggleAutostart)
        {
            Checked = IsAutostartEnabled()
        };
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

    private void OnUpdateTick(object? sender, EventArgs e)
    {
        HardwareMetrics metrics = _hardwareMonitor.GetMetrics();

        // Determina o valor a enviar no Byte 1 (mostrador físico do cooler)
        byte displayValue = 0;

        switch (_currentMode)
        {
            case DisplayMode.CpuTemp:
                displayValue = (byte)Math.Clamp((int)Math.Round(metrics.CpuTemp), 0, 99);
                break;

            case DisplayMode.GpuTemp:
                displayValue = (byte)Math.Clamp((int)Math.Round(metrics.GpuTemp), 0, 99);
                break;

            case DisplayMode.HighestTemp:
                float highest = Math.Max(metrics.CpuTemp, metrics.GpuTemp);
                displayValue = (byte)Math.Clamp((int)Math.Round(highest), 0, 99);
                break;

            case DisplayMode.Alternating:
                _alternateCounter++;
                if (_alternateCounter >= 5)
                {
                    _alternateCounter = 0;
                    _alternateShowCpu = !_alternateShowCpu;
                }
                float val = _alternateShowCpu ? metrics.CpuTemp : metrics.GpuTemp;
                displayValue = (byte)Math.Clamp((int)Math.Round(val), 0, 99);
                break;

            case DisplayMode.CpuLoad:
                displayValue = (byte)Math.Clamp((int)Math.Round(metrics.CpuLoad), 0, 99);
                break;

            case DisplayMode.RamLoad:
                displayValue = (byte)Math.Clamp((int)Math.Round(metrics.RamLoad), 0, 99);
                break;
        }

        byte cpuTemp = displayValue; // O byte 1 é o que o display exibe
        byte gpuTemp = (byte)Math.Clamp((int)Math.Round(metrics.GpuTemp), 0, 99);
        byte cpuLoad = (byte)Math.Clamp((int)Math.Round(metrics.CpuLoad), 0, 99);
        byte ramLoad = (byte)Math.Clamp((int)Math.Round(metrics.RamLoad), 0, 99);
        byte mbTemp = (byte)Math.Clamp((int)Math.Round(metrics.MotherboardTemp), 0, 99);
        byte gpuLoad = (byte)Math.Clamp((int)Math.Round(metrics.GpuLoad), 0, 99);

        bool sent = _displayController.SendPacket(cpuTemp, gpuTemp, cpuLoad, ramLoad, mbTemp, gpuLoad);

        // Log periódico dos primeiros 5 ciclos
        if (_logCycles < 5)
        {
            _logCycles++;
            File.AppendAllText(@"C:\Vizzor\vizzor.log", $"[Tick {_logCycles}] CPU={metrics.CpuTemp:F1}C, GPU={metrics.GpuTemp:F1}C, DisplayVal={displayValue}, Sent={sent}\n");
        }

        // Atualiza Tooltip da bandeja
        string status = sent ? "Conectado" : "Buscando visor...";
        string tooltip = $"Vizzor ({status})\nCPU: {metrics.CpuTemp:F0}°C ({metrics.CpuLoad:F0}%)\nGPU: {metrics.GpuTemp:F0}°C ({metrics.GpuLoad:F0}%)\nVisor: {displayValue}";

        if (tooltip.Length >= 64)
        {
            tooltip = $"Vizzor: {displayValue} | CPU: {metrics.CpuTemp:F0}°C | GPU: {metrics.GpuTemp:F0}°C";
            if (tooltip.Length >= 64)
            {
                tooltip = tooltip.Substring(0, 63);
            }
        }

        _notifyIcon.Text = tooltip;
    }

    private void OnToggleAutostart(object? sender, EventArgs e)
    {
        bool enable = !_menuAutostart.Checked;
        SetAutostart(enable);
        _menuAutostart.Checked = enable;
    }

    private static bool IsAutostartEnabled()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);
            return key?.GetValue(AppName) != null;
        }
        catch
        {
            return false;
        }
    }

    private static void SetAutostart(bool enable)
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
            if (key != null)
            {
                if (enable)
                {
                    string exePath = Environment.ProcessPath ?? Application.ExecutablePath;
                    key.SetValue(AppName, $"\"{exePath}\"");
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Falha ao configurar inicialização automática: {ex.Message}", "Vizzor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LoadSettings()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(AppRegistryKey, false);
            if (key?.GetValue("DisplayMode") is int modeVal && Enum.IsDefined(typeof(DisplayMode), modeVal))
            {
                _currentMode = (DisplayMode)modeVal;
            }
        }
        catch { }
    }

    private void SaveSettings()
    {
        try
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(AppRegistryKey);
            key.SetValue("DisplayMode", (int)_currentMode);
        }
        catch { }
    }

    private static Icon CreateAppIcon()
    {
        // Cria um ícone simples e elegante de 16x16 pixels com a letra 'V' em ciano
        using var bmp = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.Transparent);
            using var brush = new SolidBrush(Color.FromArgb(0, 180, 255));
            using var font = new Font("Segoe UI", 10, FontStyle.Bold, GraphicsUnit.Pixel);
            g.DrawString("V", font, brush, new PointF(3, 1));
        }
        IntPtr hIcon = bmp.GetHicon();
        return Icon.FromHandle(hIcon);
    }

    private void OnExit(object? sender, EventArgs e)
    {
        _updateTimer.Stop();
        _notifyIcon.Visible = false;
        _displayController.Dispose();
        _hardwareMonitor.Dispose();
        _notifyIcon.Dispose();
        ExitThread();
    }
}
