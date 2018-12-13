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
        public TimeSpan InventoryScanInterval { get; }
        public GenAntennaConfiguration GenAntennaConfiguration { get; }
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
            InventoryScanInterval = TimeSpan.FromMilliseconds(data[offset + 7] * 100);
            GenAntennaConfiguration = (GenAntennaConfiguration)data[offset + 8];
            BuzzerEnabled = data[offset + 9] > 0;
            // 10 is reserved
            AntennaCheck = (data[offset + 11] & 1) > 0;

        }
    }
}