using System;

namespace OrgPalThreeDemo.AwsIotCore.DeviceMessageSchemas
{
    public class TelemetryMessage
    {
#pragma warning disable IDE1006 // Naming Styles, disabled due to being Json specific
        public string serialNumber { get; set; }
        public DateTime sendTimestamp { get; set; }
        public int messageNumber { get; set; }
        public float batteryVoltage { get; set; }

        public float enclosureTemperature { get; set; } //will need to change to double
        public float mcuTemperature { get; set; } //will need to change to double
        public uint memoryFree { get; set; }
        public double airTemperature { get; set; } = double.NaN;
        public double thermistorTemperature { get; set; } = double.NaN;
#pragma warning restore IDE1006 // Naming Styles
    }
}
