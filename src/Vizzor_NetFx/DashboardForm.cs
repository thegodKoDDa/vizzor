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
        private Label _lblSsdTemp;

        private RadioButton _rbCpu;
        private RadioButton _rbGpu;
        private RadioButton _rbSsd;
        private RadioButton _rbMax;
        private RadioButton _rbAlt;
        private RadioButton _rbClock;

        private CheckBox _chkManualTest;
        private TrackBar _sliderManualTest;
        private Label _lblSliderVal;

        private Button _btnLang;
        private Button _btnUsbScan;

        public bool IsManualTestActive
        {
            get { return _chkManualTest != null && _chkManualTest.Checked; }
        }

        public byte ManualTestValue
        {
            get { return _sliderManualTest != null ? (byte)_sliderManualTest.Value : (byte)0; }
        }

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
            this.Text = Strings.AppTitle;
            this.Size = new Size(560, 560);
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
                Text = _deviceManager.ActiveDevice.DeviceName,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(160, 180, 200),
                AutoSize = true,
                Location = new Point(100, 18)
            };

            // Botao Idioma (PT / EN)
            _btnLang = new Button
            {
                Text = Strings.CurrentLanguage == Language.PtBr ? "🇧🇷 PT" : "🇺🇸 EN",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 50, 65),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(68, 28),
                Location = new Point(460, 12)
            };
            _btnLang.FlatAppearance.BorderSize = 0;
            _btnLang.Click += (s, e) =>
            {
                Strings.CurrentLanguage = Strings.CurrentLanguage == Language.PtBr ? Language.EnUs : Language.PtBr;
                _btnLang.Text = Strings.CurrentLanguage == Language.PtBr ? "🇧🇷 PT" : "🇺🇸 EN";
                UpdateLanguageStrings();
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(_lblHeaderDevice);
            pnlHeader.Controls.Add(_btnLang);
            this.Controls.Add(pnlHeader);

            // Container Central
            // 1. Visor Simulado (Water Cooler Head)
            var pnlCoolerHead = new Panel
            {
                Location = new Point(25, 70),
                Size = new Size(175, 175),
                BackColor = Color.FromArgb(12, 14, 18)
            };
            pnlCoolerHead.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(0, 190, 255), 3))
                {
                    e.Graphics.DrawEllipse(pen, 5, 5, 164, 164);
                }
                using (var penInner = new Pen(Color.FromArgb(40, 50, 65), 1))
                {
                    e.Graphics.DrawEllipse(penInner, 15, 15, 144, 144);
                }
            };

            _lblDisplayTitle = new Label
            {
                Text = "CPU TEMP",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(140, 160, 180),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(155, 20),
                Location = new Point(10, 32)
            };
            _lblDisplayValue = new Label
            {
                Text = "48",
                Font = new Font("Segoe UI", 42f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 240, 255),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(155, 72),
                Location = new Point(10, 52)
            };
            var lblUnit = new Label
            {
                Text = "°C",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 190, 255),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(155, 20),
                Location = new Point(10, 126)
            };
            pnlCoolerHead.Controls.Add(_lblDisplayTitle);
            pnlCoolerHead.Controls.Add(_lblDisplayValue);
            pnlCoolerHead.Controls.Add(lblUnit);
            this.Controls.Add(pnlCoolerHead);

            // 2. Cards de Sensores em Tempo Real (Direita)
            var grpSensors = new GroupBox
            {
                Text = " Sensores de Hardware ",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 255),
                Location = new Point(220, 65),
                Size = new Size(295, 180)
            };

            _lblCpuTemp = CreateMetricLabel("CPU (Processador):", "48 °C", 22, grpSensors);
            _lblCpuLoad = CreateMetricLabel("Carga da CPU:", "15 %", 48, grpSensors);
            _lblGpuTemp = CreateMetricLabel("GPU (Placa de Vídeo):", "44 °C", 74, grpSensors);
            _lblGpuLoad = CreateMetricLabel("Carga da GPU:", "5 %", 100, grpSensors);
            _lblRamLoad = CreateMetricLabel("Memória RAM:", "45 %", 126, grpSensors);
            _lblSsdTemp = CreateMetricLabel("SSD NVMe (M.2):", "38 °C", 152, grpSensors);
            this.Controls.Add(grpSensors);

            // 3. Modos de Exibição
            var grpModes = new GroupBox
            {
                Text = " Modo de Exibição no Visor ",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 255),
                Location = new Point(25, 255),
                Size = new Size(490, 110)
            };

            _rbCpu = CreateRadioButton("Temperatura da CPU", 15, 24, grpModes, DisplayMode.CpuTemp);
            _rbGpu = CreateRadioButton("Temperatura da GPU", 250, 24, grpModes, DisplayMode.GpuTemp);
            _rbSsd = CreateRadioButton("Temperatura do SSD NVMe", 15, 50, grpModes, DisplayMode.SsdTemp);
            _rbMax = CreateRadioButton("O Mais Quente (CPU/GPU)", 250, 50, grpModes, DisplayMode.HighestTemp);
            _rbAlt = CreateRadioButton("Alternar CPU e GPU", 15, 76, grpModes, DisplayMode.Alternating);
            _rbClock = CreateRadioButton("Modo Relógio (Minutos)", 250, 76, grpModes, DisplayMode.ClockMinutes);

            this.Controls.Add(grpModes);

            // 4. Teste Manual com Slider (Opcao 3)
            var grpManual = new GroupBox
            {
                Text = " Teste Manual do Visor (Slider) ",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 255),
                Location = new Point(25, 375),
                Size = new Size(490, 75)
            };

            _chkManualTest = new CheckBox
            {
                Text = "Ativar Teste Manual",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 240, 255),
                Location = new Point(15, 24),
                AutoSize = true
            };
            _sliderManualTest = new TrackBar
            {
                Minimum = 0,
                Maximum = 99,
                Value = 77,
                TickFrequency = 10,
                Location = new Point(170, 20),
                Size = new Size(250, 45),
                Enabled = false
            };
            _lblSliderVal = new Label
            {
                Text = "77",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 240, 255),
                Location = new Point(430, 24),
                AutoSize = true
            };
            _chkManualTest.CheckedChanged += (s, e) =>
            {
                _sliderManualTest.Enabled = _chkManualTest.Checked;
            };
            _sliderManualTest.ValueChanged += (s, e) =>
            {
                _lblSliderVal.Text = _sliderManualTest.Value.ToString();
            };

            grpManual.Controls.Add(_chkManualTest);
            grpManual.Controls.Add(_sliderManualTest);
            grpManual.Controls.Add(_lblSliderVal);
            this.Controls.Add(grpManual);

            // 5. Botoes Inferiores (Diagnostico USB + Minimizar)
            _btnUsbScan = new Button
            {
                Text = "🔍 Diagnóstico USB...",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 50, 65),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(170, 34),
                Location = new Point(25, 465)
            };
            _btnUsbScan.FlatAppearance.BorderSize = 0;
            _btnUsbScan.Click += OnShowUsbDiagnostic;
            this.Controls.Add(_btnUsbScan);

            var btnMinimize = new Button
            {
                Text = "Minimizar para a Bandeja",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 120, 200),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(200, 34),
                Location = new Point(315, 465)
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Click += (s, e) => this.Hide();
            this.Controls.Add(btnMinimize);

            this.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    this.Hide();
                }
            };
        }

        private void OnShowUsbDiagnostic(object sender, EventArgs e)
        {
            var devices = HidHelper.ScanAllHidDevices();
            var form = new Form
            {
                Text = Strings.UsbScannerTitle,
                Size = new Size(620, 420),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(20, 22, 28),
                ForeColor = Color.White
            };

            var list = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                BackColor = Color.FromArgb(26, 30, 38),
                ForeColor = Color.White
            };
            list.Columns.Add("Fabricante", 140);
            list.Columns.Add("Produto", 200);
            list.Columns.Add("VID", 70);
            list.Columns.Add("PID", 70);
            list.Columns.Add("Suportado", 90);

            for (int i = 0; i < devices.Count; i++)
            {
                var d = devices[i];
                var item = new ListViewItem(d.Manufacturer);
                item.SubItems.Add(d.Product);
                item.SubItems.Add(d.Vid);
                item.SubItems.Add(d.Pid);
                item.SubItems.Add(d.IsSupported ? "SIM" : "Não");
                if (d.IsSupported) item.ForeColor = Color.FromArgb(0, 240, 255);
                list.Items.Add(item);
            }

            var btnCopy = new Button
            {
                Text = "Copiar Relatório USB",
                Dock = DockStyle.Bottom,
                Height = 36,
                BackColor = Color.FromArgb(30, 120, 200),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White
            };
            btnCopy.Click += delegate
            {
                string text = "=== RELATÓRIO USB VIZZOR ===\n";
                for (int i = 0; i < devices.Count; i++)
                {
                    var d = devices[i];
                    text += string.Format("{0} | {1} | VID={2} | PID={3} | Suportado={4}\n", d.Manufacturer, d.Product, d.Vid, d.Pid, d.IsSupported);
                }
                Clipboard.SetText(text);
                MessageBox.Show("Relatório copiado para a Área de Transferência!", "Vizzor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            form.Controls.Add(list);
            form.Controls.Add(btnCopy);
            form.ShowDialog(this);
        }

        private void UpdateLanguageStrings()
        {
            this.Text = Strings.AppTitle;
            _rbCpu.Text = Strings.CpuTemp;
            _rbGpu.Text = Strings.GpuTemp;
            _rbSsd.Text = Strings.SsdTemp;
            _rbMax.Text = Strings.HighestTemp;
            _rbAlt.Text = Strings.Alternating;
            _rbClock.Text = Strings.ClockMinutes;
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
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(190, top),
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

            string status = _deviceManager.ActiveDevice != null && _deviceManager.ActiveDevice.IsConnected ? Strings.Connected : Strings.Searching;
            string devName = _deviceManager.ActiveDevice != null ? _deviceManager.ActiveDevice.DeviceName : "Visor USB";
            _lblHeaderDevice.Text = string.Format("{0} [{1}]", devName, status);

            _lblDisplayValue.Text = displayValue.ToString();
            _lblCpuTemp.Text = string.Format("{0:F1} °C", metrics.CpuTemp);
            _lblCpuLoad.Text = string.Format("{0:F0} %", metrics.CpuLoad);
            _lblGpuTemp.Text = string.Format("{0:F1} °C", metrics.GpuTemp);
            _lblGpuLoad.Text = string.Format("{0:F0} %", metrics.GpuLoad);
            _lblRamLoad.Text = string.Format("{0:F0} %", metrics.RamLoad);
            _lblSsdTemp.Text = string.Format("{0:F1} °C", metrics.SsdTemp > 0 ? metrics.SsdTemp : 38f);

            switch (currentMode)
            {
                case DisplayMode.CpuTemp: _lblDisplayTitle.Text = "CPU TEMP"; break;
                case DisplayMode.GpuTemp: _lblDisplayTitle.Text = "GPU TEMP"; break;
                case DisplayMode.SsdTemp: _lblDisplayTitle.Text = "SSD NVME"; break;
                case DisplayMode.HighestTemp: _lblDisplayTitle.Text = "HOTSPOT"; break;
                case DisplayMode.Alternating: _lblDisplayTitle.Text = "ALTERNANDO"; break;
                case DisplayMode.ClockMinutes: _lblDisplayTitle.Text = "RELÓGIO"; break;
                case DisplayMode.StealthOff: _lblDisplayTitle.Text = "DESLIGADO"; break;
                default: _lblDisplayTitle.Text = "VIZZOR"; break;
            }

            if (displayValue >= 80)
                _lblDisplayValue.ForeColor = Color.FromArgb(255, 60, 60);
            else if (displayValue >= 65)
                _lblDisplayValue.ForeColor = Color.FromArgb(255, 175, 40);
            else
                _lblDisplayValue.ForeColor = Color.FromArgb(0, 240, 255);

            _rbCpu.Checked = currentMode == DisplayMode.CpuTemp;
            _rbGpu.Checked = currentMode == DisplayMode.GpuTemp;
            _rbSsd.Checked = currentMode == DisplayMode.SsdTemp;
            _rbMax.Checked = currentMode == DisplayMode.HighestTemp;
            _rbAlt.Checked = currentMode == DisplayMode.Alternating;
            _rbClock.Checked = currentMode == DisplayMode.ClockMinutes;
        }
    }
}
