using System;
using System.Threading;
using System.Windows.Forms;

namespace Vizzor;

internal static class Program
{
    private const string AppGuid = "Global\\Vizzor_WaterCooler_AuraIce_SingleInstance_Mutex";

    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var mutex = new Mutex(initiallyOwned: true, AppGuid, out bool isOnlyInstance);

        if (!isOnlyInstance)
        {
            MessageBox.Show("O Vizzor já está em execução na bandeja do sistema.", "Vizzor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Application.Run(new TrayApplicationContext());
    }
}
