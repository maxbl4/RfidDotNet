using System;
using System.Globalization;
using System.Net;
using System.Xml;
using maxbl4.Infrastructure.Extensions.DateTimeExt;
using maxbl4.Infrastructure.Extensions.XmlExt;

namespace maxbl4.RfidDotNet.AlienTech.TagStream
{
    public static class ReaderInfoParser
    {
        public static bool ParseLine(this ReaderInfo info, string msg)
        {
            if (msg.StartsWith("#Alien RFID Reader Tag Stream", StringComparison.OrdinalIgnoreCase))
                return true;
            if (msg.StartsWith("#ReaderName:", StringComparison.OrdinalIgnoreCase))
            {
                if (GetValue(msg, out var v))
                {
                    info.ReaderName = v;
                    return true;
                }
            }

            if (msg.StartsWith("#Hostname:", StringComparison.OrdinalIgnoreCase))
            {
                if (GetValue(msg, out var v))
                {
                    info.Hostname = v;
                    return true;
                }
            }

            if (msg.StartsWith("#IPAddress:", StringComparison.OrdinalIgnoreCase))
            {
                if (GetValue(msg, out var v) && IPAddress.TryParse(v, out var ipAddress))
                {
                    info.IPAddress = ipAddress;
                    return true;
                }
            }

            if (msg.StartsWith("#IPAddress6:", StringComparison.OrdinalIgnoreCase))
            {
                if (GetValue(msg, out var t) && IPAddress.TryParse(t, out var ipAddress6))
                {
                    info.IPAddress6 = ipAddress6;
                    return true;
                }
            }

            if (msg.StartsWith("#CommandPort:", StringComparison.OrdinalIgnoreCase))
            {
                if (GetValue(msg, out var t) && int.TryParse(t, out var commandPort))
                {
                    info.CommandPort = commandPort;
                    return true;
                }
            }

            if (msg.StartsWith("#MACAddress:", StringComparison.OrdinalIgnoreCase))
            {
                if (GetValue(msg, out var v))
                {
                    info.MACAddress = v;
                    return true;
                }
            }

            if (msg.StartsWith("#Time:", StringComparison.OrdinalIgnoreCase))
            {
                if (GetValue(msg, out var t) && DateTimeExt.TryParseAsUtc(t,
                        "yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out var time))
                {
                    info.Time = time;
                    return true;
                }
            }

            return false;
        }

        public static ReaderInfo FromXmlString(XmlNode root)
        {
            var ri = new ReaderInfo();
            ri.ReaderName = root.Attr("//ReaderName");
            if (IPAddress.TryParse(root.Attr("//IPAddress"), out var ipAddress))
                ri.IPAddress = ipAddress;
            if (IPAddress.TryParse(root.Attr("//IPv6Address"), out var ipAddress6))
                ri.IPAddress6 = ipAddress6;
            if (int.TryParse(root.Attr("//CommandPort"), out var commandPort))
                ri.CommandPort = commandPort;
            ri.MACAddress = root.Attr("//MACAddress");
            ri.Time = DateTimeOffset.UtcNow;
            return ri;
        }

        static bool GetValue(string msg, out string value)
        {
            value = null;
            var ind = msg.IndexOf(": ", StringComparison.OrdinalIgnoreCase);
            if (ind < 0) return false;
            value = msg.Substring(ind + 2).Trim();
            return true;
        }
    }
}