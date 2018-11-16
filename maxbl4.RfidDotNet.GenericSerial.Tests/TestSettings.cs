using Microsoft.Extensions.Configuration;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    public class TestSettings
    {
        public string PortName { get; set; }

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