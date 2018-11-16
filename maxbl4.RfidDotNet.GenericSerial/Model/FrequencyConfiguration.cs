using System;

namespace maxbl4.RfidDotNet.GenericSerial.Model
{
    public class FrequencyConfiguration
    {
        public byte MinFreq { get; }
        public byte MaxFreq { get; }

        public FrequencyConfiguration(byte[] data, int offset)
        {
            if (data.Length - offset < 2)
                throw new ArgumentException("Data too small. Must at least 2 bytes");
            MaxFreq = data[offset];
            MinFreq = data[offset + 1];
        }
    }
}