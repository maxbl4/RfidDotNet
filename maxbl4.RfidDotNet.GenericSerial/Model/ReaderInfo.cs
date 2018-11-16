using System;

namespace maxbl4.RfidDotNet.GenericSerial.Model
{
    public class ReaderInfo
    {
        public Version FirmwareVersion { get; }
        public ReaderModel Model { get; }
        public ProtocolType SupportedProtocols { get; }
        public FrequencyConfiguration FrequencyConfiguration { get; }
        public byte RFPower { get; }
        public TimeSpan InventoryInterval { get; }
        public AntennaConfiguration AntennaConfiguration { get; }
        public bool BuzzerEnabled { get; }
        public bool AntennaCheck { get; }

        public ReaderInfo(byte[] data, int offset)
        {
            if (data.Length - offset < 12) throw new ArgumentException("Data too small. Must at least 12 bytes");
            FirmwareVersion = new Version(data[offset], data[offset + 1]);
            Model = (ReaderModel)data[offset + 2];
            SupportedProtocols = (ProtocolType)data[offset + 3];
            FrequencyConfiguration = new FrequencyConfiguration(data, offset + 4);
            RFPower = data[offset + 6];
            InventoryInterval = TimeSpan.FromMilliseconds(data[offset + 7] * 100);
            AntennaConfiguration = (AntennaConfiguration)data[offset + 8];
            BuzzerEnabled = data[offset + 9] > 0;
            // 10 is reserved
            AntennaCheck = (data[offset + 11] & 1) > 0;

        }
    }

    public enum ReaderModel: byte
    {
        RRU9813M = 0x0f,
        CF_RU5202 = 0x10,
        UHFReader288MP = 0x20 
    }

    [Flags]
    public enum AntennaConfiguration: byte
    {
        Nothing = 0,
        Antenna1 = 0b0000_0001,
        Antenna2 = 0b0000_0010,
        Antenna3 = 0b0000_0100,
        Antenna4 = 0b0000_1000,
        ResetOnPowerOff = 0b1000_0000
    }
}