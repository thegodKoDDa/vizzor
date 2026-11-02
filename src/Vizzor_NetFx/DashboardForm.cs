using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Vizzor
{
    public sealed class DashboardForm : Form
    {
        private readonly HardwareMonitor _hardwareMonitor;
        private readonly DeviceManager _deviceManager;
        private readonly Action<DisplayMode> _onModeChanged;
        private readonly Func<DisplayMode> _getMode;

        private Label _lblHeaderDevice;
        private Label _lblDisplayValue;
        private Label _lblDisplayTitle;
        private Label _lblCpuTemp;
        private Label _lblCpuLoad;
        private Label _lblGpuTemp;
        private Label _lblGpuLoad;
        private Label _lblRamLoad;

        private RadioButton _rbCpu;
        private RadioButton _rbGpu;
        private RadioButton _rbMax;
        private RadioButton _rbAlt;
        private RadioButton _rbClock;
        private RadioButton _rbStealth;

        public DashboardForm(HardwareMonitor monitor, DeviceManager deviceManager, Action<DisplayMode> onModeChanged, Func<DisplayMode> getMode)
        {
            _hardwareMonitor = monitor;
            _deviceManager = deviceManager;
            _onModeChanged = onModeChanged;
            _getMode = getMode;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Vizzor - Painel de Controle Multi-Cooler";
            this.Size = new Size(540, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(20, 22, 28);
            this.ForeColor = Color.White;

            // Painel Superior / Header
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.FromArgb(26, 30, 38)
            };
            var lblTitle = new Label
            {
                Text = "VIZZOR",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 220, 255),
                AutoSize = true,
                Location = new Point(20, 14)
            };
            _lblHeaderDevice = new Label
            {
                Text = "Dispositivo: " + _deviceManager.ActiveDevice.DeviceName,
                Font = new Font("Segoe UI", 8f, FontStyle.Regular),
                ForeColor = Color.FromArgb(160, 180, 200),
                AutoSize = true,
                Location = new Point(100, 18)
            };
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(_lblHeaderDevice);
            this.Controls.Add(pnlHeader);

            // Container Central
            // 1. Visor Simulado (Water Cooler Cooler Head Visual)
            var pnlCoolerHead = new Panel
            {
                Location = new Point(30, 70),
                Size = new Size(180, 180),
                BackColor = Color.FromArgb(12, 14, 18)
            };
            pnlCoolerHead.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(0, 190, 255), 3))
                {
                    e.Graphics.DrawEllipse(pen, 6, 6, 166, 166);
                }
                using (var penInner = new Pen(Color.FromArgb(40, 50, 65), 1))
                {
                    e.Graphics.DrawEllipse(penInner, 16, 16, 146, 146);
                }
            };

            _lblDisplayTitle = new Label
            {
                Text = "CPU TEMP",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(140, 160, 180),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(160, 20),
                Location = new Point(10, 35)
            };
            _lblDisplayValue = new Label
            {
                Text = "48",
                Font = new Font("Segoe UI", 44f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 240, 255),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(160, 75),
                Location = new Point(10, 55)
            };
            var lblUnit = new Label
            {
                Text = "°C",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 190, 255),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(160, 20),
                Location = new Point(10, 130)
            };
            pnlCoolerHead.Controls.Add(_lblDisplayTitle);
            pnlCoolerHead.Controls.Add(_lblDisplayValue);
            pnlCoolerHead.Controls.Add(lblUnit);
            this.Controls.Add(pnlCoolerHead);

            // 2. Cards de Sensores em Tempo Real (Direita)
            var grpSensors = new GroupBox
            {
                Text = " Sensores de Hardware ",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 255),
                Location = new Point(230, 65),
                Size = new Size(260, 185)
            };

            _lblCpuTemp = CreateMetricLabel("CPU (Ryzen 5 5600X):", "48 °C", 25, grpSensors);
            _lblCpuLoad = CreateMetricLabel("Carga da CPU:", "15 %", 55, grpSensors);
            _lblGpuTemp = CreateMetricLabel("GPU (RTX 5060 Ti):", "44 °C", 85, grpSensors);
            _lblGpuLoad = CreateMetricLabel("Carga da GPU:", "5 %", 115, grpSensors);
            _lblRamLoad = CreateMetricLabel("Uso de Memória RAM:", "45 %", 145, grpSensors);
            this.Controls.Add(grpSensors);

            // 3. Modos de Exibição (Inferior)
            var grpModes = new GroupBox
            {
                Text = " Modo de Exibição no Visor ",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 255),
                Location = new Point(30, 265),
                Size = new Size(460, 115)
            };

            _rbCpu = CreateRadioButton("Temperatura da CPU", 20, 28, grpModes, DisplayMode.CpuTemp);
            _rbGpu = CreateRadioButton("Temperatura da GPU", 230, 28, grpModes, DisplayMode.GpuTemp);
            _rbMax = CreateRadioButton("O Mais Quente (CPU/GPU)", 20, 56, grpModes, DisplayMode.HighestTemp);
            _rbAlt = CreateRadioButton("Alternar CPU e GPU", 230, 56, grpModes, DisplayMode.Alternating);
            _rbClock = CreateRadioButton("Modo Relógio (Minutos)", 20, 84, grpModes, DisplayMode.ClockMinutes);
            _rbStealth = CreateRadioButton("Modo Stealth (Apagado)", 230, 84, grpModes, DisplayMode.StealthOff);

            this.Controls.Add(grpModes);

            // Botão Fechar / Minimizar para bandeja
            var btnMinimize = new Button
            {
                Text = "Minimizar para a Bandeja",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 120, 200),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(200, 34),
                Location = new Point(160, 395)
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Click += (s, e) => this.Hide();
            this.Controls.Add(btnMinimize);

            // Ao fechar pelo 'X', apenas esconde
            this.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    this.Hide();
                }
            };
        }

        private Label CreateMetricLabel(string title, string initialValue, int top, Control parent)
        {
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(170, 185, 200),
                Location = new Point(15, top),
                AutoSize = true
            };
            var lblVal = new Label
            {
                Text = initialValue,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(165, top),
                AutoSize = true
            };
            parent.Controls.Add(lblTitle);
            parent.Controls.Add(lblVal);
            return lblVal;
        }

        private RadioButton CreateRadioButton(string text, int x, int y, Control parent, DisplayMode mode)
        {
            var rb = new RadioButton
            {
                Text = text,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(x, y),
                AutoSize = true
            };
            rb.CheckedChanged += (s, e) =>
            {
                if (rb.Checked) _onModeChanged(mode);
            };
            parent.Controls.Add(rb);
            return rb;
        }

        public void UpdateTelemetry(HardwareMetrics metrics, byte displayValue, DisplayMode currentMode)
        {
            if (!this.Visible) return;

            string status = _deviceManager.ActiveDevice.IsConnected ? "Conectado" : "Buscando...";
            _lblHeaderDevice.Text = string.Format("{0} [{1}]", _deviceManager.ActiveDevice.DeviceName, status);

            _lblDisplayValue.Text = displayValue.ToString();
            _lblCpuTemp.Text = string.Format("{0:F1} °C", metrics.CpuTemp);
            _lblCpuLoad.Text = string.Format("{0:F0} %", metrics.CpuLoad);
            _lblGpuTemp.Text = string.Format("{0:F1} °C", metrics.GpuTemp);
            _lblGpuLoad.Text = string.Format("{0:F0} %", metrics.GpuLoad);
            _lblRamLoad.Text = string.Format("{0:F0} %", metrics.RamLoad);

            switch (currentMode)
            {
                case DisplayMode.CpuTemp: _lblDisplayTitle.Text = "CPU TEMP"; break;
                case DisplayMode.GpuTemp: _lblDisplayTitle.Text = "GPU TEMP"; break;
                case DisplayMode.HighestTemp: _lblDisplayTitle.Text = "HOTSPOT"; break;
                case DisplayMode.Alternating: _lblDisplayTitle.Text = "ALTERNANDO"; break;
                case DisplayMode.ClockMinutes: _lblDisplayTitle.Text = "RELÓGIO"; break;
                case DisplayMode.StealthOff: _lblDisplayTitle.Text = "DESLIGADO"; break;
                default: _lblDisplayTitle.Text = "VIZZOR"; break;
            }

            // Cores dinâmicas no display simulado
            if (displayValue >= 80)
                _lblDisplayValue.ForeColor = Color.FromArgb(255, 60, 60);
            else if (displayValue >= 65)
                _lblDisplayValue.ForeColor = Color.FromArgb(255, 175, 40);
            else
                _lblDisplayValue.ForeColor = Color.FromArgb(0, 240, 255);

            // Sincroniza radio buttons
            _rbCpu.Checked = currentMode == DisplayMode.CpuTemp;
            _rbGpu.Checked = currentMode == DisplayMode.GpuTemp;
            _rbMax.Checked = currentMode == DisplayMode.HighestTemp;
            _rbAlt.Checked = currentMode == DisplayMode.Alternating;
            _rbClock.Checked = currentMode == DisplayMode.ClockMinutes;
            _rbStealth.Checked = currentMode == DisplayMode.StealthOff;
        }
    }
}
