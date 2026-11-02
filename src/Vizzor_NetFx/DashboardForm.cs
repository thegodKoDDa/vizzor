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
        private Label _lblUnit;

        // Telemetria em Tempo Real
        private Label _lblCpuTemp;
        private Label _lblCpuDetail;
        private Label _lblGpuTemp;
        private Label _lblGpuDetail;
        private Label _lblRamLoad;
        private Label _lblSsdTemp;
        private Label _lblMbDetail;

        // Radio Buttons de Modos
        private RadioButton _rbCpu;
        private RadioButton _rbGpu;
        private RadioButton _rbSsd;
        private RadioButton _rbMax;
        private RadioButton _rbCpuLoad;
        private RadioButton _rbRamLoad;
        private RadioButton _rbAlt;
        private RadioButton _rbClock;

        private CheckBox _chkManualTest;
        private TrackBar _sliderManualTest;
        private Label _lblSliderVal;

        private Button _btnLang;
        private Button _btnUsbScan;
        private GroupBox _grpSensors;
        private GroupBox _grpModes;
        private GroupBox _grpManual;

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
            this.Size = new Size(660, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(20, 22, 28);
            this.ForeColor = Color.White;

            // Painel Superior / Header
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(26, 30, 38)
            };
            var lblTitle = new Label
            {
                Text = "VIZZOR",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 220, 255),
                AutoSize = true,
                Location = new Point(20, 16)
            };
            _lblHeaderDevice = new Label
            {
                Text = (_deviceManager.ActiveDevice != null && _deviceManager.ActiveDevice.IsConnected)
                    ? _deviceManager.ActiveDevice.DeviceName
                    : Strings.StandaloneModeTitle,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = (_deviceManager.ActiveDevice != null && _deviceManager.ActiveDevice.IsConnected)
                    ? Color.FromArgb(0, 220, 255)
                    : Color.FromArgb(100, 225, 160),
                AutoSize = true,
                Location = new Point(105, 19)
            };

            // Botão Idioma (PT / EN)
            _btnLang = new Button
            {
                Text = Strings.CurrentLanguage == Language.PtBr ? "🇧🇷 PT" : "🇺🇸 EN",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 50, 65),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(72, 28),
                Location = new Point(555, 14)
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

            // 1. Visor Simulado (Water Cooler Head / Mostrador Digital)
            var pnlCoolerHead = new Panel
            {
                Location = new Point(25, 75),
                Size = new Size(185, 235),
                BackColor = Color.FromArgb(12, 14, 18)
            };
            pnlCoolerHead.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(0, 190, 255), 3))
                {
                    e.Graphics.DrawEllipse(pen, 10, 15, 164, 164);
                }
                using (var penInner = new Pen(Color.FromArgb(40, 50, 65), 1))
                {
                    e.Graphics.DrawEllipse(penInner, 20, 25, 144, 144);
                }
            };

            _lblDisplayTitle = new Label
            {
                Text = "CPU TEMP",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(140, 160, 180),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(165, 20),
                Location = new Point(10, 44)
            };
            _lblDisplayValue = new Label
            {
                Text = "48",
                Font = new Font("Segoe UI", 42f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 240, 255),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(165, 72),
                Location = new Point(10, 64)
            };
            _lblUnit = new Label
            {
                Text = "°C",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 190, 255),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(165, 20),
                Location = new Point(10, 138)
            };

            var lblDisplaySub = new Label
            {
                Text = "Visualizador Digital",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(120, 140, 160),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(175, 24),
                Location = new Point(5, 195)
            };

            pnlCoolerHead.Controls.Add(_lblDisplayTitle);
            pnlCoolerHead.Controls.Add(_lblDisplayValue);
            pnlCoolerHead.Controls.Add(_lblUnit);
            pnlCoolerHead.Controls.Add(lblDisplaySub);
            this.Controls.Add(pnlCoolerHead);

            // 2. Cards de Sensores em Tempo Real (Direita)
            _grpSensors = new GroupBox
            {
                Text = Strings.HardwareSensorsGroup,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 255),
                Location = new Point(225, 70),
                Size = new Size(400, 240)
            };

            // CPU
            _lblCpuTemp = CreatePrimaryMetricLabel("CPU:", "48.0 °C (15%)", 24, _grpSensors);
            _lblCpuDetail = CreateSecondaryMetricLabel("Ryzen 5 5600X • 4.2 GHz • 65 W", 44, _grpSensors);

            // GPU
            _lblGpuTemp = CreatePrimaryMetricLabel("GPU:", "44.0 °C (5%)", 68, _grpSensors);
            _lblGpuDetail = CreateSecondaryMetricLabel("RTX 5060 Ti • VRAM: 4.5/16 GB • 140 W", 88, _grpSensors);

            // RAM
            _lblRamLoad = CreatePrimaryMetricLabel("RAM:", "15.4 / 32.0 GB (48%)", 112, _grpSensors);

            // Armazenamento
            _lblSsdTemp = CreatePrimaryMetricLabel("SSD / Armazenamento:", "38.0 °C", 140, _grpSensors);

            // Placa-Mãe & Ventoinhas
            _lblMbDetail = CreatePrimaryMetricLabel("Placa-Mãe & Fans:", "34.0 °C • CPU Fan: 1250 RPM", 175, _grpSensors);

            this.Controls.Add(_grpSensors);

            // 3. Modos de Exibição
            _grpModes = new GroupBox
            {
                Text = Strings.DisplayModeTitle,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 255),
                Location = new Point(25, 325),
                Size = new Size(600, 130)
            };

            _rbCpu = CreateRadioButton("Temperatura da CPU", 15, 24, _grpModes, DisplayMode.CpuTemp);
            _rbGpu = CreateRadioButton("Temperatura da GPU", 210, 24, _grpModes, DisplayMode.GpuTemp);
            _rbSsd = CreateRadioButton("SSD NVMe", 410, 24, _grpModes, DisplayMode.SsdTemp);

            _rbMax = CreateRadioButton("O Mais Quente", 15, 54, _grpModes, DisplayMode.HighestTemp);
            _rbCpuLoad = CreateRadioButton("Uso da CPU (%)", 210, 54, _grpModes, DisplayMode.CpuLoad);
            _rbRamLoad = CreateRadioButton("Uso da RAM (%)", 410, 54, _grpModes, DisplayMode.RamLoad);

            _rbAlt = CreateRadioButton("Alternar CPU e GPU", 15, 84, _grpModes, DisplayMode.Alternating);
            _rbClock = CreateRadioButton("Modo Relógio (Minutos)", 210, 84, _grpModes, DisplayMode.ClockMinutes);

            this.Controls.Add(_grpModes);

            // 4. Teste Manual com Slider (Opcao 3)
            _grpManual = new GroupBox
            {
                Text = Strings.ManualTestTitle,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 255),
                Location = new Point(25, 470),
                Size = new Size(600, 80)
            };

            _chkManualTest = new CheckBox
            {
                Text = Strings.CurrentLanguage == Language.PtBr ? "Ativar Teste Manual" : "Enable Manual Test",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 240, 255),
                Location = new Point(15, 28),
                AutoSize = true
            };
            _sliderManualTest = new TrackBar
            {
                Minimum = 0,
                Maximum = 99,
                Value = 77,
                TickFrequency = 10,
                Location = new Point(190, 24),
                Size = new Size(330, 45),
                Enabled = false
            };
            _lblSliderVal = new Label
            {
                Text = "77",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 240, 255),
                Location = new Point(535, 26),
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

            _grpManual.Controls.Add(_chkManualTest);
            _grpManual.Controls.Add(_sliderManualTest);
            _grpManual.Controls.Add(_lblSliderVal);
            this.Controls.Add(_grpManual);

            // 5. Botões Inferiores (Diagnóstico USB + Minimizar)
            _btnUsbScan = new Button
            {
                Text = Strings.CurrentLanguage == Language.PtBr ? "🔍 Diagnóstico USB..." : "🔍 USB Diagnostic...",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 50, 65),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(200, 36),
                Location = new Point(25, 570)
            };
            _btnUsbScan.FlatAppearance.BorderSize = 0;
            _btnUsbScan.Click += OnShowUsbDiagnostic;
            this.Controls.Add(_btnUsbScan);

            var btnMinimize = new Button
            {
                Text = Strings.CurrentLanguage == Language.PtBr ? "Minimizar para a Bandeja" : "Minimize to Tray",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 120, 200),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(220, 36),
                Location = new Point(405, 570)
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
                Text = Strings.CurrentLanguage == Language.PtBr ? "Copiar Relatório USB" : "Copy USB Report",
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
                MessageBox.Show(
                    Strings.CurrentLanguage == Language.PtBr ? "Relatório copiado para a Área de Transferência!" : "Report copied to Clipboard!",
                    "Vizzor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            form.Controls.Add(list);
            form.Controls.Add(btnCopy);
            form.ShowDialog(this);
        }

        private void UpdateLanguageStrings()
        {
            this.Text = Strings.AppTitle;
            _grpSensors.Text = Strings.HardwareSensorsGroup;
            _grpModes.Text = Strings.DisplayModeTitle;
            _grpManual.Text = Strings.ManualTestTitle;
            _rbCpu.Text = Strings.CpuTemp;
            _rbGpu.Text = Strings.GpuTemp;
            _rbSsd.Text = Strings.SsdTemp;
            _rbMax.Text = Strings.HighestTemp;
            _rbCpuLoad.Text = Strings.CpuLoad;
            _rbRamLoad.Text = Strings.RamLoad;
            _rbAlt.Text = Strings.Alternating;
            _rbClock.Text = Strings.ClockMinutes;
            _chkManualTest.Text = Strings.CurrentLanguage == Language.PtBr ? "Ativar Teste Manual" : "Enable Manual Test";
            _btnUsbScan.Text = Strings.CurrentLanguage == Language.PtBr ? "🔍 Diagnóstico USB..." : "🔍 USB Diagnostic...";
        }

        private Label CreatePrimaryMetricLabel(string title, string initialValue, int top, Control parent)
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
                Location = new Point(160, top),
                AutoSize = true
            };
            parent.Controls.Add(lblTitle);
            parent.Controls.Add(lblVal);
            return lblVal;
        }

        private Label CreateSecondaryMetricLabel(string text, int top, Control parent)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(130, 150, 175),
                Location = new Point(160, top),
                AutoSize = true
            };
            parent.Controls.Add(lbl);
            return lbl;
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

            // Cabeçalho de Status
            if (_deviceManager.ActiveDevice != null && _deviceManager.ActiveDevice.IsConnected)
            {
                _lblHeaderDevice.Text = "🧊 " + _deviceManager.ActiveDevice.DeviceName;
                _lblHeaderDevice.ForeColor = Color.FromArgb(0, 220, 255);
            }
            else
            {
                _lblHeaderDevice.Text = Strings.StandaloneModeTitle;
                _lblHeaderDevice.ForeColor = Color.FromArgb(100, 225, 160);
            }

            _lblDisplayValue.Text = displayValue.ToString();

            // CPU
            _lblCpuTemp.Text = string.Format("{0:F1} °C ({1:F0}%)", metrics.CpuTemp, metrics.CpuLoad);
            string cpuName = !string.IsNullOrEmpty(metrics.CpuName) ? metrics.CpuName : "CPU";
            string cpuClock = metrics.CpuClockGhz > 0 ? string.Format("{0:F2} GHz", metrics.CpuClockGhz) : "";
            string cpuPower = metrics.CpuPowerWatts > 0 ? string.Format("{0:F0} W", metrics.CpuPowerWatts) : "";
            string cpuVolt = metrics.CpuVoltage > 0 ? string.Format("{0:F2} V", metrics.CpuVoltage) : "";

            string cpuExtra = cpuName;
            if (!string.IsNullOrEmpty(cpuClock)) cpuExtra += " • " + cpuClock;
            if (!string.IsNullOrEmpty(cpuPower)) cpuExtra += " • " + cpuPower;
            if (!string.IsNullOrEmpty(cpuVolt)) cpuExtra += " • " + cpuVolt;
            _lblCpuDetail.Text = cpuExtra;

            // GPU
            string gpuHot = metrics.GpuHotspotTemp > 0 ? string.Format(" (Hotspot: {0:F0}°C)", metrics.GpuHotspotTemp) : "";
            _lblGpuTemp.Text = string.Format("{0:F1} °C ({1:F0}%){2}", metrics.GpuTemp, metrics.GpuLoad, gpuHot);

            string gpuName = !string.IsNullOrEmpty(metrics.GpuName) ? metrics.GpuName : "GPU";
            string gpuVram = (metrics.GpuVramUsedGb > 0 && metrics.GpuVramTotalGb > 0)
                ? string.Format("VRAM: {0:F1}/{1:F1} GB", metrics.GpuVramUsedGb, metrics.GpuVramTotalGb)
                : "";
            string gpuPower = metrics.GpuPowerWatts > 0 ? string.Format("{0:F0} W", metrics.GpuPowerWatts) : "";
            string gpuFan = metrics.GpuFanRpm > 0 ? string.Format("{0:F0} RPM", metrics.GpuFanRpm) : "";

            string gpuExtra = gpuName;
            if (!string.IsNullOrEmpty(gpuVram)) gpuExtra += " • " + gpuVram;
            if (!string.IsNullOrEmpty(gpuPower)) gpuExtra += " • " + gpuPower;
            if (!string.IsNullOrEmpty(gpuFan)) gpuExtra += " • Fan: " + gpuFan;
            _lblGpuDetail.Text = gpuExtra;

            // RAM
            if (metrics.RamTotalGb > 0)
            {
                _lblRamLoad.Text = string.Format("{0:F1} / {1:F1} GB ({2:F0}%)", metrics.RamUsedGb, metrics.RamTotalGb, metrics.RamLoad);
            }
            else
            {
                _lblRamLoad.Text = string.Format("{0:F0} %", metrics.RamLoad);
            }

            // Armazenamento
            string ssdName = !string.IsNullOrEmpty(metrics.StorageName) ? metrics.StorageName + " • " : "";
            _lblSsdTemp.Text = string.Format("{0}{1:F1} °C", ssdName, metrics.SsdTemp > 0 ? metrics.SsdTemp : 38f);

            // Placa-mãe & Ventoinhas
            string mbName = !string.IsNullOrEmpty(metrics.MotherboardName) ? metrics.MotherboardName : "Motherboard";
            string mbTemp = metrics.MotherboardTemp > 0 ? string.Format("{0:F0} °C", metrics.MotherboardTemp) : "OK";
            string cpuFan = metrics.CpuFanRpm > 0 ? string.Format("CPU Fan: {0:F0} RPM", metrics.CpuFanRpm) : "Fans Ativos";
            _lblMbDetail.Text = string.Format("{0} • {1} • {2}", mbName, mbTemp, cpuFan);

            // Unidade no Visor
            switch (currentMode)
            {
                case DisplayMode.CpuTemp:
                    _lblDisplayTitle.Text = "CPU TEMP";
                    _lblUnit.Text = "°C";
                    break;
                case DisplayMode.GpuTemp:
                    _lblDisplayTitle.Text = "GPU TEMP";
                    _lblUnit.Text = "°C";
                    break;
                case DisplayMode.SsdTemp:
                    _lblDisplayTitle.Text = "SSD NVME";
                    _lblUnit.Text = "°C";
                    break;
                case DisplayMode.HighestTemp:
                    _lblDisplayTitle.Text = "HOTSPOT";
                    _lblUnit.Text = "°C";
                    break;
                case DisplayMode.CpuLoad:
                    _lblDisplayTitle.Text = "CPU LOAD";
                    _lblUnit.Text = "%";
                    break;
                case DisplayMode.RamLoad:
                    _lblDisplayTitle.Text = "RAM LOAD";
                    _lblUnit.Text = "%";
                    break;
                case DisplayMode.Alternating:
                    _lblDisplayTitle.Text = "ALTERNANDO";
                    _lblUnit.Text = "°C";
                    break;
                case DisplayMode.ClockMinutes:
                    _lblDisplayTitle.Text = "RELÓGIO";
                    _lblUnit.Text = "MIN";
                    break;
                case DisplayMode.StealthOff:
                    _lblDisplayTitle.Text = "DESLIGADO";
                    _lblUnit.Text = "--";
                    break;
                default:
                    _lblDisplayTitle.Text = "VIZZOR";
                    _lblUnit.Text = "°C";
                    break;
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
            _rbCpuLoad.Checked = currentMode == DisplayMode.CpuLoad;
            _rbRamLoad.Checked = currentMode == DisplayMode.RamLoad;
            _rbAlt.Checked = currentMode == DisplayMode.Alternating;
            _rbClock.Checked = currentMode == DisplayMode.ClockMinutes;
        }
    }
}
