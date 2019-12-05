using PowerArgs;

namespace maxbl4.RfidDotNet.GenericSerial.Demo
{
    
    public class DemoArgs
    {
        [ArgPosition(0)]
        [ArgDescription("How to connect to reader: " +
                        "\tprotocolType=GenericSerial;SerialPortName=COM4 or protocolType=GenericSerial;SerialPortName=/dev/ttyS2" +
                        "\ttcp://my_host:1234")]
        public string ConnectionString { get; set; }
        
        [ArgDescription("How perform inventory: " +
                                  "\tPoll - to poll for tags in a loop" +
                                  "\tRealtime - to setup realtime tag stream from reader")]
        [ArgDefaultValue(InventoryType.Poll)]
        public InventoryType Inventory { get; set; }
        
        [ArgDescription("Wait for user confirmation before starting inventory")]
        [ArgDefaultValue(false)]
        public bool Confirm { get; set; }
        
        [ArgDescription("Temperature in celsius above which inventory will be stopped to prevent thermal damage")]
        [ArgDefaultValue(60)]
        public int ThermalLimit { get; set; }
        
        [ArgDefaultValue(1000)]
        public int StatsSamplingInterval { get; set; }
        [ArgDefaultValue(false)]
        public bool EnableDrmMode { get; set; }
        [ArgDefaultValue("")]
        public string TagIdFilter { get; set; }
    }

    public enum InventoryType
    {
        Poll,
        Realtime
    }
}