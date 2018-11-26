using System;

namespace maxbl4.RfidDotNet.GenericSerial.Model
{
    [Flags]
    public enum QFlags : byte
    {
        None = 0,
        RequestStatisticsPacket = 0b1000_0000,
        SpecialStrategy = 0b0100_0000,
        ImpinjFastId = 0b0010_0000
    }
}