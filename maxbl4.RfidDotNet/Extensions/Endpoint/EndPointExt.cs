using System.Net;

namespace maxbl4.RfidDotNet.Extensions.Endpoint
{
    public static class EndPointExt
    {
        public static DnsEndPoint ParseDnsEndPoint(this string str, int defaultPort)
        {
            if (str == null) return null;
            var ind = str.LastIndexOf(':');
            if (ind < 0)
                return new DnsEndPoint(str, defaultPort);
            return new DnsEndPoint(str.Substring(0, ind), int.Parse(str.Substring(ind + 1)));
        }
        
        public static SerialEndpoint ParseSerialEndpoint(this string str, int defaultBaudRate)
        {
            if (str == null) return null;
            var ind = str.LastIndexOf('@');
            if (ind < 0)
                return new SerialEndpoint(str, defaultBaudRate);
            return new SerialEndpoint(str.Substring(0, ind), int.Parse(str.Substring(ind + 1)));
        }
    }
}