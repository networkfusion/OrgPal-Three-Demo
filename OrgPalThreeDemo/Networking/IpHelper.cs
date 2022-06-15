using System;
using System.Net;
using System.Diagnostics;

namespace OrgPalThreeDemo.Networking
{
    public static class IpHelper
    {
        public static IPAddress GetPublicIpAddress(string server = "http://icanhazip.com")//or should this be GetPublicWanIpAddress ?
        {
            //var externalIpString = new WebClient().DownloadString(server).Replace("\\r\\n", "").Replace("\\n", "").Trim();
            //var externalIp = IPAddress.Parse(externalIpString);

            return new IPAddress(0);
        }

        public static int Ping(string address)
        {
            // lets look at https://github.com/dotnet/runtime/tree/main/src/libraries/System.Net.Ping/src/System/Net/NetworkInformation
            return int.MaxValue;
        }
    }
}
