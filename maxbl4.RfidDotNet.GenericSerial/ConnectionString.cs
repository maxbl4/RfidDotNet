using System;
using maxbl4.RfidDotNet.GenericSerial.DataAdapters;

namespace maxbl4.RfidDotNet.GenericSerial
{
    public class ConnectionString
    {
        public const string SerialScheme = "serial";
        public const string TcpScheme = "tcp";
        
        public string Hostname { get; set; }
        public int TcpPort { get; set; }
        public string SerialPort { get; set; }
        public ConnectionType Type { get; set; }

        public IDataStreamFactory Connect()
        {
            switch (Type)
            {
                case ConnectionType.Serial:
                    return new SerialPortFactory(SerialPort);
                case ConnectionType.Network:
                    return new NetworkStreamFactory(Hostname, TcpPort);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static ConnectionString Parse(string src)
        {
            var result = new ConnectionString();
            var u = new Uri(src);
            switch (u.Scheme)
            {
                case SerialScheme:
                    result.Type = ConnectionType.Serial;
                    result.SerialPort = u.OriginalString.Substring(u.Scheme.Length + 3);
                    break;
                case TcpScheme:
                    result.Type = ConnectionType.Network;
                    result.Hostname = u.Host;
                    result.TcpPort = u.Port;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(src), $"Only serial and tcp schemes are supported, was {u}");
            }
            
            return result;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case ConnectionType.Network:
                    return $"tcp://{Hostname}:{TcpPort}";
                case ConnectionType.Serial:
                    return $"tcp://{SerialPort}";
            }

            return "invalid://connection/string";
        }
    }
    
    public enum ConnectionType
    {
        Any,
        Serial,
        Network
    }
}