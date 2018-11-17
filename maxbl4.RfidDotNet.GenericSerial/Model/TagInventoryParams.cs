using System;

namespace maxbl4.RfidDotNet.GenericSerial.Model
{
    public class TagInventoryParams
    {
        public const int BaseSize = 8;
        public const int OptionalParamsSize = 3;
        public const byte DefaultQValue = 4;
        public const SessionValue DefaultSession = SessionValue.S0;
        public const MaskMemoryAreaType DefaultMaskMemoryMemoryArea = MaskMemoryAreaType.EPC;

        /// <summary>
        /// Range is 0-15. The value should be: 2^Q = Expected number of tags in field
        /// </summary>
        public byte QValue { get; set; } = DefaultQValue;
        /// <summary>
        /// Reader specific flags
        /// </summary>
        public TagInventoryQFlags QFlags { get; set; }

        public SessionValue Session { get; set; } = DefaultSession;
        public MaskMemoryAreaType MaskMemoryArea { get; set; } = DefaultMaskMemoryMemoryArea;
        /// <summary>
        /// Entry bit address of the mask, the valid range of MaskAdr is 0 ~ 16383
        /// </summary>
        public short MaskAddress { get; set; }
        /// <summary>
        /// Length of mask in bits
        /// </summary>
        public byte MaskLength { get; set; }

        /// <summary>
        /// Should be MaskLength/8. If mask length is not multiple of 8, value should be padded with zero bits
        /// </summary>
        public byte[] MaskData { get; set; }
        
        /// <summary>
        /// Entry address of inventory TID memory
        /// </summary>
        public byte TIDAddress { get; set; }
        
        /// <summary>
        /// Data length for TID inventory operation, the valid range of LenTID is 0 ~ 15
        /// </summary>
        public byte TIDLength { get; set; }

        public TagInventoryOptionalParams OptionalParams { get; set; }

        public byte[] Serialize()
        {
            var bufferSize = BaseSize;
            if (MaskLength > 0)
            {
                if (MaskData == null)
                    throw new ArgumentNullException(nameof(MaskData), "MaskData must be set if MaskLength is > 0");
                var maskLenDiff = MaskData.Length * 8 - MaskLength;
                if (maskLenDiff < 0 || maskLenDiff > 7)
                    throw new ArgumentOutOfRangeException(nameof(MaskLength), "The length of MaskData equals to MaskLen/8. " +
                                                                              "If MaskLen is not a multiple of 8 integer, the length of " +
                                                                              "MaskData is equal to the int[MaskLen/8]+1. Non-specified " +
                                                                              "lower significant figures should be filled up with 0");
                bufferSize += MaskData.Length;
            }

            if (OptionalParams != null)
                bufferSize += OptionalParamsSize;

            var buf = new byte[bufferSize];
            buf[0] = (byte)(QValue | (byte)QFlags);
            buf[1] = (byte) Session;
            buf[2] = (byte) MaskMemoryArea;
            buf[3] = (byte) MaskAddress;
            buf[4] = (byte) (MaskAddress >> 8);
            buf[5] = MaskLength;
            var offset = 6;
            if (MaskData != null)
            {
                Array.Copy(MaskData, 0, buf, offset, MaskData.Length);
                offset += MaskData.Length;
            }

            buf[offset] = TIDAddress;
            buf[offset + 1] = TIDLength;

            if (OptionalParams != null)
            {
                buf[offset + 2] = (byte)OptionalParams.Target;
                buf[offset + 3] = (byte)OptionalParams.Antenna;
                buf[offset + 4] = (byte)(OptionalParams.ScanTime.TotalMilliseconds/100);
            }

            return buf;
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

    public enum MaskMemoryAreaType
    {
        EPC = 0x1,
        TID = 0x2,
        User = 0x3,
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