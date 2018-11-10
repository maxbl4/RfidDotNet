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
        public DateTimeOffset Time { get; set; }
        
        public IPEndPoint EndPoint => new IPEndPoint(IPAddress, CommandPort);
    }
}