using System;

namespace OrgPalThreeDemo
{
    public class StatusMessage
    {
        public string operatingSystem { get; set; }
        public string platform { get; set; }
        public string cpu { get; set; }
        public string serialNumber { get; set; }
        public DateTime sendTimestamp { get; set; }
        public DateTime bootTimestamp { get; set; }
        public int messageNumber { get; set; }
        public float batteryVoltage { get; set; }
        public float enclosureTemperature { get; set; }
        public float mcuTemperature { get; set; }
        public uint memoryFree { get; set; }
        public float airTemperature { get; set; }
    }
}
