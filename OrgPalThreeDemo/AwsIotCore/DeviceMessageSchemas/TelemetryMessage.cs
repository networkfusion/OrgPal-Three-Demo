using System;

namespace OrgPalThreeDemo.AwsIotCore.DeviceMessageSchemas
{
    public class TelemetryMessage
    {
#pragma warning disable IDE1006 // Naming Styles, disabled due to being Json specific
        public int schemaVersion { get; set; } = 1;
        public DateTime sendTimestamp { get; set; } = DateTime.UtcNow;
        public uint messageNumber { get; set; } = 0;

        // main board specific
        public string serialNumber { get; set; } = "";
        public double batteryVoltage { get; set; } = double.NaN;
        public double pcbTemperatureCelsius { get; set; } = double.NaN;
        public double mcuTemperatureCelsius { get; set; } = double.NaN;

        // runtime information
        public uint memoryFreeBytes { get; set; } = 0; // used for monitoring memory leaks

        // expansion board peripherals
        public double pt100TemperatureCelsius { get; set; } = double.NaN;
        public double thermistorTemperatureCelsius { get; set; } = double.NaN;
#pragma warning restore IDE1006 // Naming Styles
    }
}
