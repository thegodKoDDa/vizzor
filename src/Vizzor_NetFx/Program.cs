using System;
using System.Threading;
using System.Windows.Forms;

namespace Vizzor
{
    internal static class Program
    {
        private const string AppGuid = "Global\\Vizzor_WaterCooler_AuraIce_SingleInstance_Mutex";

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool isOnlyInstance;
            using (var mutex = new Mutex(true, AppGuid, out isOnlyInstance))
            {
                if (!isOnlyInstance)
                {
                    MessageBox.Show("O Vizzor ja esta em execucao na bandeja do sistema.", "Vizzor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Application.Run(new TrayApplicationContext());
            }
        }
    }
}
