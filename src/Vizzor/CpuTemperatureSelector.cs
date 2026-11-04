using System;

namespace Vizzor
{
    internal static class CpuTemperatureSelector
    {
        // Temperatura do die/package antes de CCDs ou nucleos individuais.
        internal static int Priority(string name)
        {
            if (name.IndexOf("Tctl/Tdie", StringComparison.OrdinalIgnoreCase) >= 0) return 6;
            if (name.IndexOf("CCD", StringComparison.OrdinalIgnoreCase) >= 0) return 2;
            if (name.IndexOf("Tdie", StringComparison.OrdinalIgnoreCase) >= 0) return 5;
            if (name.IndexOf("Package", StringComparison.OrdinalIgnoreCase) >= 0) return 4;
            if (name.IndexOf("Tctl", StringComparison.OrdinalIgnoreCase) >= 0) return 3;
            return 1;
        }
    }
}
