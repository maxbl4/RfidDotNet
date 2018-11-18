using System;

namespace maxbl4.RfidDotNet.GenericSerial.Model
{
    //TODO: Implement support for Mask memory. This requires traffic capturing from original driver,
    //since mask formatting is not trivial and not completely documented in spec. 
    public class TagInventoryParams
    {
        public const int BaseSize = 2;
        public const int OptionalParamsSize = 3;
        public const byte DefaultQValue = 4;
        public const SessionValue DefaultSession = SessionValue.S0;

        /// <summary>
        /// Range is 0-15. The value should be: 2^Q = Expected number of tags in field
        /// </summary>
        public byte QValue { get; set; } = DefaultQValue;
        /// <summary>
        /// Reader specific flags
        /// </summary>
        public TagInventoryQFlags QFlags { get; set; }

        public SessionValue Session { get; set; } = DefaultSession;

        public TagInventoryOptionalParams OptionalParams { get; set; }

        public byte[] Serialize()
        {
            var bufferSize = BaseSize;
            if (OptionalParams != null) bufferSize += OptionalParamsSize;

            var offset = 0;
            var buf = new byte[bufferSize];
            buf[offset++] = (byte)(QValue | (byte)QFlags);
            buf[offset++] = (byte) Session;

            if (OptionalParams != null)
            {
                buf[offset++] = (byte)OptionalParams.Target;
                buf[offset++] = (byte)OptionalParams.Antenna;
                buf[offset++] = (byte)(OptionalParams.ScanTime.TotalMilliseconds/100);
            }

            return buf;
        }
    }

    public class TagInventoryWithBufferParams : TagInventoryParams
    {
        public TagInventoryWithBufferParams(TagInventoryOptionalParams requiredParams)
        {
            OptionalParams = requiredParams;
        }
    }

    public class TagInventoryOptionalParams
    {
        public const EPCTarget DefaultEPCTarget = EPCTarget.A;
        public const InventoryAntenna DefaultAntenna = InventoryAntenna.Antenna1;
        public EPCTarget Target { get; set; }
        public InventoryAntenna Antenna { get; set; }
        public TimeSpan ScanTime { get; set; }

        public TagInventoryOptionalParams(TimeSpan scanTime, EPCTarget target = DefaultEPCTarget, InventoryAntenna antenna = DefaultAntenna)
        {
            ScanTime = scanTime;
            Target = target;
            Antenna = antenna;
        }
    }

    public enum InventoryAntenna : byte
    {
        Antenna1 = 0x80,
        Antenna2 = 0x81,
        Antenna3 = 0x82,
        Antenna4 = 0x83,
    }

    public enum EPCTarget : byte
    {
        A = 0x0,
        B = 0x1
    }

    [Flags]
    public enum TagInventoryQFlags : byte
    {
        RequestStatisticsPacket = 0b1000_0000,
        SpecialStrategy = 0b0100_0000,
        ImpinjFastId = 0b0010_0000
    }

    public enum SessionValue : byte
    {
        S0 = 0x0,
        S1 = 0x1,
        S2 = 0x2,
        S3 = 0x3,
        UseSmartConfiguration = 0xff
    }
}