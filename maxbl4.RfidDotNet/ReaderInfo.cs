using System;
using System.Net;

namespace maxbl4.RfidDotNet
{
    public class ReaderInfo
    {
        public string ReaderName { get; set; }
        public string Hostname { get; set; }
        public string COMPort { get; set; }
        public IPAddress IPAddress { get; set; }
        public IPAddress IPAddress6 { get; set; }
        public int CommandPort { get; set; }
        public string MACAddress { get; set; }
        public DateTime Time { get; set; } = new(0, DateTimeKind.Utc);
        
        public IPEndPoint EndPoint => new(IPAddress, CommandPort);
    }
}