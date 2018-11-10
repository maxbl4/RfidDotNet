using System;
using System.Globalization;
using System.Net;
using System.Xml;
using maxbl4.RfidDotNet.AlienTech.Ext;

namespace maxbl4.RfidDotNet.AlienTech.TagStream
{
    public class ReaderInfo
    {
        private string readerName;
        private string hostname;
        private IPAddress ipAddress;
        private IPAddress ipAddress6;
        private int commandPort;
        private string macAddress;
        private DateTimeOffset time;
        public string ReaderName => readerName;
        public string Hostname => hostname;
        public IPAddress IPAddress => ipAddress;
        public IPAddress IPAddress6 => ipAddress6;
        public int CommandPort => commandPort;
        public string MACAddress => macAddress;
        public DateTimeOffset Time => time;
        
        public IPEndPoint EndPoint => new IPEndPoint(IPAddress, CommandPort);

        public bool ParseLine(string msg)
        {
            if (msg.StartsWith("#Alien RFID Reader Tag Stream", StringComparison.OrdinalIgnoreCase))
                return true;
            if (msg.StartsWith("#ReaderName:", StringComparison.OrdinalIgnoreCase))
                return GetValue(msg, out readerName);
            if (msg.StartsWith("#Hostname:", StringComparison.OrdinalIgnoreCase))
                return GetValue(msg, out hostname);
            if (msg.StartsWith("#IPAddress:", StringComparison.OrdinalIgnoreCase))
                return GetValue(msg, out var t) && IPAddress.TryParse(t, out ipAddress);
            if (msg.StartsWith("#IPAddress6:", StringComparison.OrdinalIgnoreCase))
                return GetValue(msg, out var t) && IPAddress.TryParse(t, out ipAddress6);
            if (msg.StartsWith("#CommandPort:", StringComparison.OrdinalIgnoreCase))
                return GetValue(msg, out var t) && int.TryParse(t, out commandPort);
            if (msg.StartsWith("#MACAddress:", StringComparison.OrdinalIgnoreCase))
                return GetValue(msg, out macAddress);
            if (msg.StartsWith("#Time:", StringComparison.OrdinalIgnoreCase))
                return GetValue(msg, out var t) && DateTimeExt.TryParseAsUtc(t,
                           "yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture,
                           DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out time);
            return false;
        }

        public static ReaderInfo FromXmlString(XmlNode root)
        {
            var ri = new ReaderInfo();
            ri.readerName = root.Attr("//ReaderName"); 
            IPAddress.TryParse(root.Attr("//IPAddress"), out ri.ipAddress);
            IPAddress.TryParse(root.Attr("//IPv6Address"), out ri.ipAddress6);
            int.TryParse(root.Attr("//CommandPort"), out ri.commandPort);
            ri.macAddress = root.Attr("//MACAddress");
            ri.time = DateTimeOffset.UtcNow;
            return ri;
        }

        bool GetValue(string msg, out string value)
        {
            value = null;
            var ind = msg.IndexOf(": ", StringComparison.OrdinalIgnoreCase);
            if (ind < 0) return false;
            value = msg.Substring(ind + 2).Trim();
            return true;
        }
    }
}