using System;
using System.IO;

namespace Vizzor
{
    internal static class AppLog
    {
        private static readonly object Sync = new object();
        internal static readonly string DirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vizzor");

        internal static void Write(string message)
        {
            lock (Sync)
            {
                try
                {
                    Directory.CreateDirectory(DirectoryPath);
                    string path = Path.Combine(DirectoryPath, "vizzor.log");
                    if (File.Exists(path) && new FileInfo(path).Length > 1048576)
                        File.WriteAllText(path, "");
                    File.AppendAllText(path, DateTime.Now.ToString("s") + " " + message + Environment.NewLine);
                }
                catch { }
            }
        }
    }
}
