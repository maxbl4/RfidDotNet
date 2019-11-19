using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using maxbl4.Infrastructure.Extensions.IPAddressExt;

namespace maxbl4.RfidDotNet.AlienTech.Net
{
    public static class EndpointLookup
    {
        public static IPAddress GetIpOnTheSameNet(IPAddress refAddress)
        {
            var interfaces = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(x => x.OperationalStatus == OperationalStatus.Up &&
                            x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(x => x.GetIPProperties());
            foreach (var i in interfaces)
            {
                foreach (var ip4 in i.UnicastAddresses.Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork))
                {
                    if (ip4.Address.Mask(ip4.IPv4Mask).Equals(refAddress.Mask(ip4.IPv4Mask)))
                        return ip4.Address;
                }
            }
            throw new Exception("Failed to find local address");
        }
        
        public static IPAddress GetAnyPhysicalIp()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(x => x.OperationalStatus == OperationalStatus.Up && x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                .Select(x => x.Address)
                .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
        }
        
    }
    
    
}