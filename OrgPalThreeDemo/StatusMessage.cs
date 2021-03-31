using System;

namespace OrgPalThreeDemo
{
    public class StatusMessage
    {
#pragma warning disable IDE1006 // Naming Styles
        public string serialNumber { get; set; }
        public DateTime sendTimestamp { get; set; }
        public int messageNumber { get; set; }
        public float batteryVoltage { get; set; }

        public float enclosureTemperature { get; set; }
        public float mcuTemperature { get; set; }
        public uint memoryFree { get; set; }
        public float airTemperature { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
