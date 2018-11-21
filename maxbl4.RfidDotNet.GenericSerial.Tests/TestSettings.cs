using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using maxbl4.RfidDotNet.GenericSerial.DataAdapters;
using Microsoft.Extensions.Configuration;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    public class TestSettings
    {
        public List<ReaderConnection> Connections { get; set; }
        public string KnownTagIds { get; set; }

        public IDataStreamFactory GetConnection()
        {
            var c = Connections.FirstOrDefault(x => x.Type == ConnectionType.Serial);
            if (c != null) return new SerialPortFactory(c.Params); 
            c = Connections.FirstOrDefault(x => x.Type == ConnectionType.Network);
            if (c != null)
            {
                var ind = c.Params.LastIndexOf(':');
                var ep = new IPEndPoint(IPAddress.Parse(c.Params.Substring(0, ind)), int.Parse(c.Params.Substring(ind + 1)));
                return new NetworkStreamFactory(ep);
            }
            return null;
        }

        public IEnumerable<string> GetKnownTagIds => string.IsNullOrWhiteSpace(KnownTagIds)
            ? new string[0]
            : KnownTagIds.Split(new char[]{',', ' ', ';'}, StringSplitOptions.RemoveEmptyEntries); 

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

        public enum ConnectionType
        {
            Serial,
            Network
        }
    }
}