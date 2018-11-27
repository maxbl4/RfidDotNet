using maxbl4.RfidDotNet.GenericSerial.Model;
using PowerArgs;

namespace maxbl4.RfidDotNet.GenericSerial.Demo
{
    
    public class DemoArgs
    {
        [ArgPosition(0)]
        [ArgDescription("How to connect to reader: " +
                        "\tserial://COM4 or serial:///dev/ttyS2" +
                        "\ttcp://my_host:1234")]
        public string ConnectionString { get; set; }
        
        [ArgDescription("How perform inventory: " +
                                  "\tPoll - to poll for tags in a loop" +
                                  "\tRealtime - to setup realtime tag stream from reader")]
        [ArgDefaultValue(InventoryType.Poll)]
        public InventoryType Inventory { get; set; }
        
        [ArgDescription("Set RF power for inventory. Valid range depends on reader model, typical range 0 to 30")]
        [ArgRange(0, 30)]
        [ArgDefaultValue(10)]
        public byte RFPower { get; set; }
        
        [ArgDescription("Wait for user confirmation before starting inventory")]
        [ArgDefaultValue(false)]
        public bool Confirm { get; set; }
        
        [ArgDescription("Temperature in celsius above which inventory will be stopped to prevent thermal damage")]
        [ArgDefaultValue(60)]
        public int ThermalLimit { get; set; }
        
        [ArgDefaultValue(57600)]
        public int SerialBaudRate { get; set; }

        [ArgDefaultValue(4)]
        public byte QValue { get; set; }
        [ArgDefaultValue((byte)0)]
        public byte Session { get; set; }
        [ArgDefaultValue(2000)]
        public int ScanInterval { get; set; }
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