using System;

namespace OrgPalThreeDemo.AwsIotCore.DeviceMessageSchemas
{
    public class ShadowStateProperties
    {
#pragma warning disable IDE1006 // Naming Styles, disabled due to being Json specific
        public string operatingSystem { get; set; } = string.Empty;
        public string platform { get; set; } = string.Empty;
        public string cpu { get; set; } = string.Empty;
        public string serialNumber { get; set; } = string.Empty;
        public DateTime bootTimestamp { get; set; } = new DateTime();
        public string endpointAddressIpv4 { get; set; } = string.Empty;
        // public string endpointAddressIpv6 { get; set; } = string.Empty;
        // public string endpointGeoLocation { get; set; } = string.Empty;
#pragma warning restore IDE1006 // Naming Styles
    }
}
