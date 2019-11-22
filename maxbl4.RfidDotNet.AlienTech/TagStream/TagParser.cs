using System;
using System.Globalization;
using System.Text;

namespace maxbl4.RfidDotNet.AlienTech.TagStream
{
    public static class TagParser
    {
        public static string CustomFormat => "%k;${MSEC1};${MSEC2};%a;%m;%r";

        public static Tag Parse(string msg)
        {
            var parts = SanitizeString(msg).Split(new []{';'}, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                return new Tag
                {
                    TagId = parts[0],
                    DiscoveryTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(parts[1])).UtcDateTime,
                    LastSeenTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(parts[2])).UtcDateTime,
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
                DiscoveryTime = DateTimeOffset.FromUnixTimeMilliseconds(discoveryMsec).UtcDateTime,
                LastSeenTime = DateTimeOffset.FromUnixTimeMilliseconds(lastSeenMsec).UtcDateTime,
                Antenna = int.Parse(parts[3]),
                Rssi = double.Parse(parts[4], CultureInfo.InvariantCulture),
                ReadCount = int.Parse(parts[5])
            };
            return true;
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

        public static string ToCustomFormatString(this Tag tag)
        {
            return $"{tag.TagId};{new DateTimeOffset(tag.DiscoveryTime).ToUnixTimeMilliseconds()};{new DateTimeOffset(tag.LastSeenTime).ToUnixTimeMilliseconds()};{tag.Antenna};{tag.Rssi.ToString(CultureInfo.InvariantCulture)};{tag.ReadCount}";
        }
    }
}