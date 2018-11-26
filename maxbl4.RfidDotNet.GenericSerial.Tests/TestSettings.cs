using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using maxbl4.RfidDotNet.GenericSerial.DataAdapters;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using maxbl4.RfidDotNet.GenericSerial.Model;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    public class TestSettings
    {
        public string ConnectionStrings { get; set; }
        public string KnownTagIds { get; set; }

        public IDataStreamFactory GetConnection(ConnectionType type = ConnectionType.Any, BaudRates baudRate = BaudRates.Baud57600)
        {
            ConnectionString cs = null;
            switch (type)
            {
                case ConnectionType.Any:
                    cs = GetConnectionStrings().FirstOrDefault();
                    break;
                case ConnectionType.Network:
                case ConnectionType.Serial:
                    cs = GetConnectionStrings().FirstOrDefault(x => x.Type == type);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (cs != null)
            {
                switch (cs.Type)
                {
                    case ConnectionType.Serial:
                        return new SerialPortFactory(cs.SerialPort, baudRate.ToNumber());
                    case ConnectionType.Network:
                        return new NetworkStreamFactory(cs.Hostname, cs.TcpPort);
                }
            }
            
            Skip.If(true);

            return null;
        }

        public IEnumerable<ConnectionString> GetConnectionStrings() =>
            ConnectionStrings.Split(new []{';'}, StringSplitOptions.RemoveEmptyEntries).Select(ConnectionString.Parse);

        public IEnumerable<string> GetKnownTagIds => string.IsNullOrWhiteSpace(KnownTagIds)
            ? new string[0]
            : KnownTagIds.Split(new []{',', ' ', ';'}, StringSplitOptions.RemoveEmptyEntries); 

        private TestSettings()
        {
        }

        public static readonly TestSettings Instance;

        static TestSettings()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("test-settings.json", true)
                .AddEnvironmentVariables()
                .Build();
            config.Bind(Instance = new TestSettings());
        }

        public class ReaderConnection
        {
            public ConnectionType Type {get;set;}
            public string Params {get;set;}
        }
    }
}