using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    public class TestSettings
    {
        public string PortName { get; set; }
        public string KnownTagIds { get; set; }

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
    }
}