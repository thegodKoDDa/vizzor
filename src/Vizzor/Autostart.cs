using System;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Vizzor
{
    internal static class Autostart
    {
        private const string LegacyKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private static string TaskName { get { return "Vizzor-" + WindowsIdentity.GetCurrent().User.Value; } }

        private static dynamic Connect()
        {
            dynamic service = Activator.CreateInstance(Type.GetTypeFromProgID("Schedule.Service"));
            service.Connect();
            return service;
        }

        internal static bool IsEnabled()
        {
            try { return (bool)Connect().GetFolder("\\").GetTask(TaskName).Enabled; }
            catch { return false; }
        }

        internal static void SetEnabled(bool enable)
        {
            dynamic service = Connect();
            dynamic folder = service.GetFolder("\\");
            if (enable)
            {
                string sid = WindowsIdentity.GetCurrent().User.Value;
                dynamic task = service.NewTask(0);
                task.RegistrationInfo.Description = "Vizzor: sensores e visor USB ao entrar no Windows";
                task.Principal.UserId = sid;
                task.Principal.LogonType = 3; // InteractiveToken: mostra a bandeja na sessao do usuario.
                task.Principal.RunLevel = 1; // Highest: necessario para os sensores de CPU.
                dynamic trigger = task.Triggers.Create(9); // Logon
                trigger.UserId = sid;
                trigger.Delay = "PT15S";
                dynamic action = task.Actions.Create(0);
                action.Path = Application.ExecutablePath;
                action.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                task.Settings.DisallowStartIfOnBatteries = false;
                task.Settings.StopIfGoingOnBatteries = false;
                task.Settings.ExecutionTimeLimit = "PT0S";
                task.Settings.StartWhenAvailable = true;
                task.Settings.MultipleInstances = 2; // IgnoreNew
                task.Settings.RestartCount = 3;
                task.Settings.RestartInterval = "PT1M";
                folder.RegisterTaskDefinition(TaskName, task, 6, sid, null, 3, null);
            }
            else if (IsEnabled()) folder.DeleteTask(TaskName, 0);

            using (var key = Registry.CurrentUser.OpenSubKey(LegacyKey, true))
                if (key != null) key.DeleteValue("Vizzor", false);
        }

        internal static void MigrateLegacy()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(LegacyKey))
                {
                    if (key == null || key.GetValue("Vizzor") == null) return;
                }
                SetEnabled(true);
                AppLog.Write("Inicializacao migrada para tarefa agendada com privilegios elevados.");
            }
            catch (Exception ex) { AppLog.Write("Falha ao migrar inicializacao: " + ex.Message); }
        }
    }
}
