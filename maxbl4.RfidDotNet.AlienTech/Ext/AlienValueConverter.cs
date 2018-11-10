using System;
using System.Globalization;
using System.Net;

namespace maxbl4.RfidDotNet.AlienTech.Ext
{
    public class AlienValueConverter
    {
        public const string AlienDateTimeFormat = "yyyy/MM/dd HH:mm:ss";
        public static string ToAlienValueString(object value)
        {
            switch (value)
            {
                case bool v: return v ? "ON" : "OFF";
                case float v: return v.ToString(CultureInfo.InvariantCulture);
                case double v: return v.ToString(CultureInfo.InvariantCulture);
                case DateTimeOffset v: return v.ToString(AlienDateTimeFormat.Replace("/", "\\/"));
                default: return value?.ToString();
            }
        }

        public static T ToStrongType<T>(string value)
        {
            if (typeof(T) == typeof(DateTimeOffset))
                return (T) (object) DateTimeExt.ParseAsUtc(value, AlienDateTimeFormat);
            if (typeof(T) == typeof(IPEndPoint)) return (T) (object)ParseEnpoint(value);
            if (typeof(T) == typeof(bool)) return (T)(object)"ON".Equals(value,StringComparison.OrdinalIgnoreCase);
            if (typeof(Enum).IsAssignableFrom(typeof(T))) return EnumExt.ParseEnum<T>(value);
            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        public static IPEndPoint ParseEnpoint(string s)
        {
            var parts = s.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries);
            var ip = IPAddress.Parse(parts[0]);
            var port = int.Parse(parts[1]);
            return new IPEndPoint(ip, port);
        }
    }
}