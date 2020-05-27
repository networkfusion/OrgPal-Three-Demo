using System;

namespace OrgPalThreeDemo
{
    public class StatusMessage
    {
        public string serialNumber { get; set; }
        public DateTime sendTimestamp { get; set; }
        public DateTime bootTimestamp { get; set; }
        public int messageNumber { get; set; }
        public float batteryVoltage { get; set; }
        public float enclosureTemperature { get; set; }
        public float mcuTemperature { get; set; }
        public int memoryFree { get; set; }
    }
}
