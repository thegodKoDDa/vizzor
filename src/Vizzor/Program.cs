using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Vizzor
{
    internal static class Program
    {
        private const string AppGuid = "Global\\Vizzor_WaterCooler_AuraIce_SingleInstance_Mutex";

        [DllImport("kernel32.dll")]
        private static extern uint SetErrorMode(uint mode);

        [STAThread]
        private static void Main(string[] args)
        {
            // Registra o resolver de assemblies embutidos antes de instanciar qualquer classe que use bibliotecas externas.
            // Isso permite que o Vizzor.exe funcione como um único arquivo autônomo (Single-File), sem exigir DLLs soltas na pasta!
            AppDomain.CurrentDomain.AssemblyResolve += ResolveEmbeddedAssembly;

            if (args.Length == 1 && args[0] == "--gpu-worker")
            {
                SetErrorMode(3); // Evita janelas de erro do driver no processo auxiliar.
                GpuMonitor.RunWorker();
                return;
            }
            if (args.Length == 1 && args[0] == "--enable-autostart")
            {
                Autostart.SetEnabled(true);
                return;
            }
            if (args.Length == 1 && args[0] == "--remove-autostart")
            {
                Autostart.SetEnabled(false);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            RunApplication();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RunApplication()
        {
            bool isOnlyInstance;
            using (var mutex = new Mutex(true, AppGuid, out isOnlyInstance))
            {
                if (!isOnlyInstance)
                {
                    MessageBox.Show(
                        Strings.CurrentLanguage == Language.PtBr
                            ? "O Vizzor já está em execução na bandeja do sistema."
                            : "Vizzor is already running in the system tray.",
                        "Vizzor",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                Autostart.MigrateLegacy();
                AppLog.Write("Iniciando " + Application.ExecutablePath);
                Application.Run(new TrayApplicationContext());
            }
        }

        private static Assembly ResolveEmbeddedAssembly(object sender, ResolveEventArgs args)
        {
            try
            {
                var requestedName = new AssemblyName(args.Name).Name;
                var currentAssembly = Assembly.GetExecutingAssembly();

                foreach (string resourceName in currentAssembly.GetManifestResourceNames())
                {
                    if (resourceName.EndsWith(requestedName + ".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var stream = currentAssembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream != null)
                            {
                                byte[] buffer = new byte[stream.Length];
                                stream.Read(buffer, 0, buffer.Length);
                                return Assembly.Load(buffer);
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }
    }
}
