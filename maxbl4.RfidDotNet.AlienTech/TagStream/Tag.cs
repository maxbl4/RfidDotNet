using System;
using System.Globalization;
using System.Text;

namespace maxbl4.RfidDotNet.AlienTech.TagStream
{
    public class Tag
    {
        public ReaderInfo Reader { get; set; }
        public string TagId { get; set; }
        public DateTimeOffset DiscoveryTime { get; set; }
        public DateTimeOffset LastSeenTime { get; set; }
        public int Antenna { get; set; }
        public int ReadCount { get; set; }
        public double Rssi { get; set; }
        public static string CustomFormat => "%k;${MSEC1};${MSEC2};%a;%m;%r";

        public static Tag Parse(string msg)
        {
            var parts = SanitizeString(msg).Split(new []{';'}, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                return new Tag
                {
                    TagId = parts[0],
                    DiscoveryTime = DtParse(long.Parse(parts[1])),
                    LastSeenTime = DtParse(long.Parse(parts[2])),
                    Antenna = int.Parse(parts[3]),
                    Rssi = double.Parse(parts[4], CultureInfo.InvariantCulture),
                    ReadCount = int.Parse(parts[5])
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not parse tag line: {msg}", ex);
            }
        }

        public static bool TryParse(string msg, out Tag tag)
        {
            tag = default;
            if (string.IsNullOrWhiteSpace(msg))
                return false;
            var parts = SanitizeString(msg).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 6)
                return false;
            if (!long.TryParse(parts[1], out var discoveryMsec)) return false;
            if (!long.TryParse(parts[2], out var lastSeenMsec)) return false;
            if (!int.TryParse(parts[3], out _)) return false;
            if (!double.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out _)) return false;
            if (!int.TryParse(parts[5], out _)) return false;
            tag = new Tag
            {
                TagId = parts[0],
                DiscoveryTime = DtParse(discoveryMsec),
                LastSeenTime = DtParse(lastSeenMsec),
                Antenna = int.Parse(parts[3]),
                Rssi = double.Parse(parts[4], CultureInfo.InvariantCulture),
                ReadCount = int.Parse(parts[5])
            };
            return true;
        }

        static DateTimeOffset DtParse(long msec)
        {
            var dt = new DateTimeOffset(1970, 01, 01, 0, 0, 0, TimeSpan.Zero);
            return dt.AddSeconds(msec / 1000d);
        }

        static string SanitizeString(string str)
        {
            var sb = new StringBuilder(str.Length);
            foreach (var c in str)
            {
                if (c >= 0x20) sb.Append(c);
            }

            return sb.ToString();
        }
    }
}