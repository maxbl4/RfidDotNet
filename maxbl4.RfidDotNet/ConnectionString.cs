using System;
using System.Collections.Generic;
using System.Net;
using maxbl4.RfidDotNet.Ext;

namespace maxbl4.RfidDotNet
{
    public class ConnectionString
    {
        public const int DefaultNetworkPort = 25;
        public const int DefaultBaudRate = 57600;
        public const int DefaultQValue = 3;
        public const int DefaultSession = 1;
        public const int DefaultRFPower = 10;
        public const int DefaultInventoryIntervalMs = 2000;
        public const AntennaConfiguration DefaultAntenna = AntennaConfiguration.Antenna1;
        public const string DefaultLogin = "alien";
        public const string DefaultPassword = "password";

        public ReaderProtocolType Protocol { get; set; }
        public DnsEndPoint Network { get; set; }
        public SerialEndpoint Serial { get; set; }

        public string Login { get; set; } = DefaultLogin;
        /// <summary>
        /// = ; symbols are not supported in a password
        /// </summary>
        public string Password { get; set; } = DefaultPassword;
        
        
        public int InventoryDuration { get; set; } = DefaultInventoryIntervalMs;
        public int QValue { get; set; } = DefaultQValue;
        public int Session { get; set; } = DefaultSession;
        public int RFPower { get; set; } = DefaultRFPower;
        public AntennaConfiguration AntennaConfiguration { get; set; } = DefaultAntenna;
        
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
                
                if (name.Equals(nameof(Protocol), StringComparison.OrdinalIgnoreCase)) 
                {
                    if (!Enum.TryParse<ReaderProtocolType>(value, true, out var parsedProtocolType))
                        throw new FormatException($"Could not parse value {value} for {name}");
                    cs.Protocol = parsedProtocolType;
                }
                if (name.Equals(nameof(Network), StringComparison.OrdinalIgnoreCase))
                    cs.Network = value.ParseDnsEndPoint(DefaultNetworkPort);
                if (name.Equals(nameof(Login), StringComparison.OrdinalIgnoreCase))
                    cs.Login = value;
                if (name.Equals(nameof(Password), StringComparison.OrdinalIgnoreCase))
                    cs.Password = value;


                if (name.Equals(nameof(Serial), StringComparison.OrdinalIgnoreCase))
                    cs.Serial = value.ParseSerialEndpoint(DefaultBaudRate);

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
            switch (Protocol)
            {
                case ReaderProtocolType.Alien:
                    if (Network == null)
                        errors.Add("Alien protocol requires Network endpoint");
                    if (string.IsNullOrEmpty(Login))
                        errors.Add("Alien protocol requires Login");
                    if (string.IsNullOrEmpty(Password))
                        errors.Add("Alien protocol requires Password");
                    if (Network != null && (Network.Port < 1 || Network.Port > 65535))
                        errors.Add("Alien protocol requires valid port on Network endpoint");
                    break;
                case ReaderProtocolType.Serial:
                    if (Network == null && Serial == null)
                        errors.Add("Serial protocol requires Serial or Network endpoint");
                    if (Network != null && (Network.Port < 1 || Network.Port > 65535))
                        errors.Add("Serial protocol requires valid Port on Network endpoint");
                    if (Serial != null && (Serial.BaudRate < 9600))
                        errors.Add("Serial protocol requires valid BaudRate on Serial endpoint");
                    break;
                default:
                    message = $"Unknown protocol type {Protocol}";
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