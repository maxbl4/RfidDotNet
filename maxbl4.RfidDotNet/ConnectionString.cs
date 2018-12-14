using System;
using System.Collections.Generic;
using System.Net;

namespace maxbl4.RfidDotNet
{
    public class ConnectionString
    {
        public const int DefaultBaudRate = 115200;
        public const int DefaultQValue = 3;
        public const int DefaultSession = 0;
        public const int DefaultRFPower = 10;
        public const int DefaultInventoryIntervalMs = 2000;
        public const AntennaConfiguration DefaultAntenna = AntennaConfiguration.Antenna1;
        public const string DefaultLogin = "alien";
        public const string DefaultPassword = "password";

        public ReaderProtocolType ProtocolType { get; set; }
        public string TcpHost { get; set; }
        public string Login { get; set; } = DefaultLogin;
        /// <summary>
        /// = ; symbols are not supported in a password
        /// </summary>
        public string Password { get; set; } = DefaultPassword;
        public int TcpPort { get; set; }
        
        public string SerialPortName { get; set; }
        public int SerialBaudRate { get; set; } = DefaultBaudRate;
        
        public int InventoryDuration { get; set; } = DefaultInventoryIntervalMs;
        public int QValue { get; set; } = DefaultQValue;
        public int Session { get; set; } = DefaultSession;
        public int RFPower { get; set; } = DefaultRFPower;
        public AntennaConfiguration AntennaConfiguration { get; set; } = DefaultAntenna;
        
        public DnsEndPoint EndPoint => new DnsEndPoint(TcpHost, TcpPort);

        public static ConnectionString Parse(string connectionString)
        {
            var cs = new ConnectionString();
            var tuples = connectionString.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var tuple in tuples)
            {
                var keyValue = tuple.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                if (keyValue.Length != 2) continue;
                var name = keyValue[0].ToLowerInvariant().Trim(' ', '\r', '\n', '\t');
                var value = keyValue[1].Trim(' ', '\r', '\n', '\t');
                int parsedInt;
                
                if (name.Equals(nameof(ProtocolType), StringComparison.OrdinalIgnoreCase)) 
                {
                    if (!Enum.TryParse<ReaderProtocolType>(value, true, out var parsedProtocolType))
                        throw new FormatException($"Could not parse value {value} for {name}");
                    cs.ProtocolType = parsedProtocolType;
                }
                if (name.Equals(nameof(TcpHost), StringComparison.OrdinalIgnoreCase))
                    cs.TcpHost = value;
                if (name.Equals(nameof(Login), StringComparison.OrdinalIgnoreCase))
                    cs.Login = value;
                if (name.Equals(nameof(Password), StringComparison.OrdinalIgnoreCase))
                    cs.Password = value;

                if (name.Equals(nameof(TcpPort), StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(value, out parsedInt))
                        throw new FormatException($"Could not parse value {value} for {name}");
                    cs.TcpPort = parsedInt;
                }


                if (name.Equals(nameof(SerialPortName), StringComparison.OrdinalIgnoreCase))
                    cs.SerialPortName = value;

                if (name.Equals(nameof(SerialBaudRate), StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(value, out parsedInt))
                        throw new FormatException($"Could not parse value {value} for {name}");
                    cs.SerialBaudRate = parsedInt;
                }


                if (name.Equals(nameof(InventoryDuration), StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(value, out parsedInt))
                        throw new FormatException($"Could not parse value {value} for {name}");
                    cs.QValue = parsedInt;
                }
                
                if (name.Equals(nameof(QValue), StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(value, out parsedInt))
                        throw new FormatException($"Could not parse value {value} for {name}");
                    cs.QValue = parsedInt;
                }

                if (name.Equals(nameof(Session), StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(value, out parsedInt))
                        throw new FormatException($"Could not parse value {value} for {name}");
                    cs.Session = parsedInt;
                }

                if (name.Equals(nameof(RFPower), StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(value, out parsedInt))
                        throw new FormatException($"Could not parse value {value} for {name}");
                    cs.RFPower = parsedInt;
                }

                if (name.Equals(nameof(AntennaConfiguration), StringComparison.OrdinalIgnoreCase))
                {
                    if (!Enum.TryParse<AntennaConfiguration>(value, true, out var parsedAntennaConfiguration))
                        throw new FormatException($"Could not parse value {value} for {name}");
                    cs.AntennaConfiguration = parsedAntennaConfiguration;
                }
            }
            return cs;
        }

        public bool IsValid(out string message)
        {
            var errors = new List<string>();
            switch (ProtocolType)
            {
                case ReaderProtocolType.Alien:
                    if (string.IsNullOrEmpty(TcpHost))
                        errors.Add("Alien protocol requires TcpHost");
                    if (string.IsNullOrEmpty(Login))
                        errors.Add("Alien protocol requires Login");
                    if (string.IsNullOrEmpty(Password))
                        errors.Add("Alien protocol requires Password");
                    if (TcpPort < 1 || TcpPort > 65535)
                        errors.Add("Alien protocol requires valid TcpPort");
                    break;
                case ReaderProtocolType.GenericSerial:
                    if (string.IsNullOrEmpty(TcpHost) && string.IsNullOrEmpty(SerialPortName))
                        errors.Add("Generic protocol requires SerialPortName or TcpHost");
                    if (!string.IsNullOrEmpty(TcpHost) && (TcpPort < 1 || TcpPort > 65535))
                        errors.Add("Generic protocol requires TcpPort with TcpHost");
                    if (!string.IsNullOrEmpty(SerialPortName) && (SerialBaudRate < 9600))
                        errors.Add("Generic protocol requires SerialBaudRate with SerialPortName");
                    break;
                default:
                    message = $"Unknown protocol type {ProtocolType}";
                    return false;
            }
            if (InventoryDuration < 1 || QValue > 25000) errors.Add($"InventoryDuration must be in range 1-25000 ms, was {InventoryDuration}");
            if (QValue < 1 || QValue > 16) errors.Add($"QValue must be in range 1-16, was {QValue}");
            if (Session < 0 || Session > 4) errors.Add($"Session must be in range 0-4, was {Session}");
            if (RFPower < 0 || RFPower > 33) errors.Add($"RFPower must be in range 0-33, was {RFPower}");
            if (AntennaConfiguration == AntennaConfiguration.Nothing) errors.Add($"AntennaConfiguration must set at least one antenna");

            message = string.Join(Environment.NewLine, errors);
            return string.IsNullOrEmpty(message);
        }

        public ConnectionString Clone()
        {
            return (ConnectionString)this.MemberwiseClone();
        }
    }
}