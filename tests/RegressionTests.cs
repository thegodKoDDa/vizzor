using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Serialization;
using System.Threading;
using LibreHardwareMonitor.Hardware;
using Vizzor;

class RegressionTests
{
    static int Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "--gpu-worker")
        {
            File.WriteAllText(Environment.GetEnvironmentVariable("VIZZOR_TEST_MARKER"), "worker exiting");
            return 23;
        }
        AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs e)
        {
            var app = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Vizzor.exe"));
            using (var stream = app.GetManifestResourceStream(new AssemblyName(e.Name).Name + ".dll"))
            {
                if (stream == null) return null;
                using (var buffer = new MemoryStream()) { stream.CopyTo(buffer); return Assembly.Load(buffer.ToArray()); }
            }
        };
        try { Run(); Console.WriteLine("PASS: CPU sensor selection, USB retry, embedded dependencies, GPU process failure isolation"); return 0; }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    class Proxy : RealProxy
    {
        readonly Dictionary<string, object> values;
        public Proxy(Type type, Dictionary<string, object> values) : base(type) { this.values = values; }
        public override IMessage Invoke(IMessage msg)
        {
            var call = (IMethodCallMessage)msg;
            object value;
            values.TryGetValue(call.MethodName, out value);
            return new ReturnMessage(value, null, 0, call.LogicalCallContext, call);
        }
    }
    static ISensor Sensor(string name, float? temperature)
    {
        return (ISensor)new Proxy(typeof(ISensor), new Dictionary<string, object> {
            {"get_Name", name}, {"get_Value", temperature}, {"get_SensorType", SensorType.Temperature}
        }).GetTransparentProxy();
    }
    static HardwareMetrics Extract(params ISensor[] sensors)
    {
        var hardware = (IHardware)new Proxy(typeof(IHardware), new Dictionary<string, object> {
            {"get_Name", "CPU test"}, {"get_Sensors", sensors}
        }).GetTransparentProxy();
        var metrics = new HardwareMetrics();
        typeof(HardwareMonitor).GetMethod("ExtractCpuMetrics", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[] {hardware, metrics});
        return metrics;
    }
    static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void Run()
    {
        Assert(Extract(Sensor("Core #1", 70), Sensor("CPU Package", 55)).CpuTemp == 55, "Package must replace first core");
        Assert(Extract(Sensor("CPU Package", 55), Sensor("Core #1", 70)).CpuTemp == 55, "Order must not change selection");
        Assert(Extract(Sensor("CCD1 (Tdie)", 43), Sensor("Core (Tctl/Tdie)", 61)).CpuTemp == 61, "Ryzen combined sensor priority");
        Assert(Extract(Sensor("Core (Tctl)", 70), Sensor("Core (Tdie)", 50)).CpuTemp == 50, "Physical die before offset control");
        Assert(Extract(Sensor("CPU Package", null), Sensor("Core #1", 49)).CpuTemp == 49, "Null sensor fallback");
        Assert(Extract(Sensor("CPU Package", float.PositiveInfinity), Sensor("Core #1", 49)).CpuTemp == 49, "Invalid sample fallback");
        Assert(Extract(Sensor("Core #1", 40), Sensor("Core #2", 60)).CpuTemp == 60, "Hottest core fallback");
        Assert(Extract(Sensor("CCD1 (Tdie)", 43), Sensor("CPU Package", 55)).CpuTemp == 55, "Package before individual CCD");
        Assert(Extract().CpuTemp == 0, "Unavailable sensor");
        var manager = (DeviceManager)FormatterServices.GetUninitializedObject(typeof(DeviceManager));
        var driver = new FakeDriver();
        typeof(DeviceManager).GetField("_supportedDrivers", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(manager, new List<IDisplayDevice> { driver });
        Assert(!manager.AutoDetect(), "Missing USB must not be reported connected");
        driver.Available = true;
        Assert(!manager.AutoDetect(), "USB retries must be throttled");
        typeof(DeviceManager).GetField("_nextDetection", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(manager, DateTime.MinValue);
        Assert(manager.AutoDetect() && manager.ActiveDevice == driver, "Late USB must connect without restart");
        driver.Available = false;
        typeof(DeviceManager).GetField("_nextDetection", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(manager, DateTime.MinValue);
        Assert(!manager.AutoDetect() && manager.ActiveDevice == null, "Disconnected USB must clear active device");
        manager.Dispose();
        string marker = Path.Combine(Path.GetTempPath(), "vizzor-gpu-test-" + Guid.NewGuid() + ".txt");
        Environment.SetEnvironmentVariable("VIZZOR_TEST_MARKER", marker);
        var gpuType = typeof(HardwareMonitor).Assembly.GetType("Vizzor.GpuMonitor");
        using (var gpu = (IDisposable)Activator.CreateInstance(gpuType,
            BindingFlags.Instance | BindingFlags.NonPublic, null, new object[0], null))
        {
            for (int i = 0; i < 50 && !File.Exists(marker); i++) Thread.Sleep(100);
            Assert(File.Exists(marker), "GPU worker process must launch");
            Thread.Sleep(300);
            Assert(Extract(Sensor("Core (Tctl/Tdie)", 51)).CpuTemp == 51,
                "CPU must remain usable after GPU worker exits unexpectedly");
        }
        if (File.Exists(marker)) File.Delete(marker);
        Environment.SetEnvironmentVariable("VIZZOR_TEST_MARKER", null);
        var resources = typeof(HardwareMonitor).Assembly.GetManifestResourceNames();
        foreach (string name in new[] {"HidSharp.dll", "RAMSPDToolkit-NDD.dll", "System.CodeDom.dll", "LibreHardwareMonitorLib.dll"})
            Assert(Array.IndexOf(resources, name) >= 0, "Missing embedded dependency " + name);
    }

    class FakeDriver : IDisplayDevice
    {
        public bool Available;
        public string DeviceName { get { return "Test cooler"; } }
        public string Manufacturer { get { return "Test"; } }
        public ushort VendorId { get { return 0; } }
        public ushort ProductId { get { return 0; } }
        public bool IsConnected { get { return Available; } }
        public bool EnsureConnected() { return Available; }
        public bool SendMetrics(byte value, HardwareMetrics metrics) { return Available; }
        public void Close() { }
        public void Dispose() { }
    }
}
