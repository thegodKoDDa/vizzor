using System;
using LibreHardwareMonitor.Hardware;

class Program
{
    static void Main()
    {
        var computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true
        };

        computer.Open();

        Console.WriteLine("=== SENSORES DETECTADOS ===");
        foreach (var hardware in computer.Hardware)
        {
            hardware.Update();
            foreach (var sub in hardware.SubHardware)
            {
                sub.Update();
            }

            Console.WriteLine($"\n[Hardware] {hardware.Name} ({hardware.HardwareType})");
            foreach (var sensor in hardware.Sensors)
            {
                Console.WriteLine($"  Sensor: {sensor.Name} | Tipo: {sensor.SensorType} | Valor: {sensor.Value}");
            }
            foreach (var sub in hardware.SubHardware)
            {
                Console.WriteLine($"  [SubHardware] {sub.Name}");
                foreach (var sensor in sub.Sensors)
                {
                    Console.WriteLine($"    Sensor: {sensor.Name} | Tipo: {sensor.SensorType} | Valor: {sensor.Value}");
                }
            }
        }
        computer.Close();
    }
}
