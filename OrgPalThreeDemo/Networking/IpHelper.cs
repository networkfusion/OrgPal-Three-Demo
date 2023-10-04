using System.Net;
using System.Net.Http;

namespace OrgPalThreeDemo.Networking
{
    public static class IpHelper
    {

        public static IPAddress GetWanIpAddress(string server = "http://icanhazip.com", bool ipv6 = false)
        {
            if (ipv6 == false)
            {
                var externalIpString = new HttpClient().GetString(server); //.Replace("\\r\\n", "").Replace("\\n", "").Trim();
                return IPAddress.Parse(externalIpString);
            }
            else
            {
                return IPAddress.GetDefaultLocalAddress(); // FIXME!
            }
        }

        public static int IcmpHostPing(string hostAddress, bool ipv6 = false)
        {
            // FIXME!
            // lets look at https://github.com/dotnet/runtime/tree/main/src/libraries/System.Net.Ping/src/System/Net/NetworkInformation
            // or http://www.java2s.com/Code/CSharp/Network/SimplePing.htm
            return int.MaxValue;
        }
    }
}
