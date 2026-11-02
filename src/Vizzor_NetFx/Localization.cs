using System;

namespace Vizzor
{
    public enum Language
    {
        PtBr,
        EnUs
    }

    public static class Strings
    {
        public static Language CurrentLanguage = Language.PtBr;

        public static string AppTitle
        {
            get { return CurrentLanguage == Language.PtBr ? "Vizzor - Painel de Controle" : "Vizzor - Control Panel"; }
        }

        public static string OpenDashboard
        {
            get { return CurrentLanguage == Language.PtBr ? "Abrir Painel de Controle..." : "Open Control Panel..."; }
        }

        public static string DisplayModeTitle
        {
            get { return CurrentLanguage == Language.PtBr ? "Modo de Exibição" : "Display Mode"; }
        }

        public static string CpuTemp
        {
            get { return CurrentLanguage == Language.PtBr ? "Temperatura da CPU" : "CPU Temperature"; }
        }

        public static string GpuTemp
        {
            get { return CurrentLanguage == Language.PtBr ? "Temperatura da GPU" : "GPU Temperature"; }
        }

        public static string SsdTemp
        {
            get { return CurrentLanguage == Language.PtBr ? "Temperatura do SSD NVMe" : "NVMe SSD Temperature"; }
        }

        public static string HighestTemp
        {
            get { return CurrentLanguage == Language.PtBr ? "O Mais Quente (CPU/GPU)" : "Highest Temp (Hotspot)"; }
        }

        public static string Alternating
        {
            get { return CurrentLanguage == Language.PtBr ? "Alternar (CPU e GPU)" : "Alternating (CPU & GPU)"; }
        }

        public static string ClockMinutes
        {
            get { return CurrentLanguage == Language.PtBr ? "Modo Relógio (Minutos)" : "Clock Mode (Minutes)"; }
        }

        public static string CpuLoad
        {
            get { return CurrentLanguage == Language.PtBr ? "Uso da CPU (%)" : "CPU Usage (%)"; }
        }

        public static string RamLoad
        {
            get { return CurrentLanguage == Language.PtBr ? "Uso de Memória RAM (%)" : "RAM Usage (%)"; }
        }

        public static string StealthMode
        {
            get { return CurrentLanguage == Language.PtBr ? "Modo Stealth (Apagar Visor)" : "Stealth Mode (Turn Off Display)"; }
        }

        public static string UpdateSpeed
        {
            get { return CurrentLanguage == Language.PtBr ? "Velocidade de Atualização" : "Update Speed"; }
        }

        public static string Fast500ms
        {
            get { return CurrentLanguage == Language.PtBr ? "Rápido (500 ms)" : "Fast (500 ms)"; }
        }

        public static string Normal1000ms
        {
            get { return CurrentLanguage == Language.PtBr ? "Normal (1 segundo)" : "Normal (1 second)"; }
        }

        public static string Eco2000ms
        {
            get { return CurrentLanguage == Language.PtBr ? "Econômico (2 segundos)" : "Eco (2 seconds)"; }
        }

        public static string AlternateInterval
        {
            get { return CurrentLanguage == Language.PtBr ? "Tempo de Alternância" : "Alternating Interval"; }
        }

        public static string VisualBlinkAlert
        {
            get { return CurrentLanguage == Language.PtBr ? "Efeito: Piscar Visor em Alta Temp (> 85°C)" : "Effect: Blink Display on High Temp (> 85°C)"; }
        }

        public static string HighTempAlert
        {
            get { return CurrentLanguage == Language.PtBr ? "Notificação no Windows (> 85°C)" : "Windows Notification (> 85°C)"; }
        }

        public static string Autostart
        {
            get { return CurrentLanguage == Language.PtBr ? "Iniciar com o Windows" : "Start with Windows"; }
        }

        public static string DisableOldSoftware
        {
            get { return CurrentLanguage == Language.PtBr ? "Desativar Software Antigo da Rise Mode" : "Disable Vendor Background Software"; }
        }

        public static string Exit
        {
            get { return CurrentLanguage == Language.PtBr ? "Sair do Vizzor" : "Exit Vizzor"; }
        }

        public static string Connected
        {
            get { return CurrentLanguage == Language.PtBr ? "Conectado" : "Connected"; }
        }

        public static string Searching
        {
            get { return CurrentLanguage == Language.PtBr ? "Buscando..." : "Searching..."; }
        }

        public static string ManualTestTitle
        {
            get { return CurrentLanguage == Language.PtBr ? "Teste Manual do Visor (Slider)" : "Manual Display Test (Slider)"; }
        }

        public static string UsbScannerTitle
        {
            get { return CurrentLanguage == Language.PtBr ? "Diagnóstico de Dispositivos USB" : "USB Devices Diagnostic"; }
        }
    }
}
