using System;
using System.Net;
using System.Net.Http;
using System.Diagnostics;

namespace OrgPalThreeDemo.Networking
{
    public static class IpHelper
    {
        public static IPAddress GetWanAddressIpv4(string server = "http://icanhazip.com")//or should this be GetPublicWanIpAddress ?
        {
            var externalIpString = new HttpClient().GetString(server); //.Replace("\\r\\n", "").Replace("\\n", "").Trim();
            return IPAddress.Parse(externalIpString);
        }

        public static int Ping(string address)
        {
            // lets look at https://github.com/dotnet/runtime/tree/main/src/libraries/System.Net.Ping/src/System/Net/NetworkInformation
            // or http://www.java2s.com/Code/CSharp/Network/SimplePing.htm
            return int.MaxValue;
        }
    }
}
