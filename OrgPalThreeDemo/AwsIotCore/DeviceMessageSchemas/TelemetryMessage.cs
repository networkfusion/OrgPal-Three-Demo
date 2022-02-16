using System;

namespace OrgPalThreeDemo.AwsIotCore.DeviceMessageSchemas
{
    public class TelemetryMessage
    {
#pragma warning disable IDE1006 // Naming Styles, disabled due to being Json specific
        public int schemaVersion { get; set; } = 1;
        public string serialNumber { get; set; } = "";
        public DateTime sendTimestamp { get; set; } = DateTime.UtcNow;
        public uint messageNumber { get; set; } = 0;
        public double batteryVoltage { get; set; } = double.NaN;

        public double enclosureTemperatureCelsius { get; set; } = double.NaN;
        public double mcuTemperatureCelsius { get; set; } = double.NaN;
        public uint memoryFreeBytes { get; set; } = 0;
        public double airTemperatureCelsius { get; set; } = double.NaN;
        public double thermistorTemperatureCelsius { get; set; } = double.NaN;
#pragma warning restore IDE1006 // Naming Styles
    }
}
