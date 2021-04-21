using System;

namespace maxbl4.RfidDotNet
{
    public class Tag
    {
        public ReaderInfo Reader { get; set; }
        public string TagId { get; set; }
        public DateTime DiscoveryTime { get; set; } = new(0, DateTimeKind.Utc);
        public DateTime LastSeenTime { get; set; } = new(0, DateTimeKind.Utc);
        public int Antenna { get; set; }
        public int ReadCount { get; set; }
        public double Rssi { get; set; }
    }
}