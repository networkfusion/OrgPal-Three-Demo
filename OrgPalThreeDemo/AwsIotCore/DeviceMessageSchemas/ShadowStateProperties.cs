using System;

namespace OrgPalThreeDemo.AwsIotCore.DeviceMessageSchemas
{
    public class ShadowStateProperties
    {
#pragma warning disable IDE1006 // Naming Styles, disabled due to being Json specific
        public string operatingSystem { get; set; }
        public string platform { get; set; }
        public string cpu { get; set; }
        public string serialNumber { get; set; }
        public DateTime bootTimestamp { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
