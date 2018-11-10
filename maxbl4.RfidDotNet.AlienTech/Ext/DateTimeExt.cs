using System;
using System.Globalization;

namespace maxbl4.RfidDotNet.AlienTech.Ext
{
    public static class DateTimeExt
    {
        public static DateTimeOffset ParseAsUtc(string value, string format)
        {
            return DateTimeOffset.ParseExact(value, format, CultureInfo.InvariantCulture)
                .ToUniversalTime().Add(TimeZoneInfo.Local.BaseUtcOffset);
        }

        public static bool TryParseAsUtc(string value, string format, CultureInfo culture, DateTimeStyles styles, out DateTimeOffset time)
        {
            var r = DateTimeOffset.TryParseExact(value,
                format, CultureInfo.InvariantCulture,
                styles, out var t);
            time = t.ToUniversalTime().Add(TimeZoneInfo.Local.BaseUtcOffset);
            return r;
        }
    }
}