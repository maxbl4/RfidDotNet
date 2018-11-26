using System;

namespace maxbl4.RfidDotNet.GenericSerial.Model
{
    [Flags]
    public enum DrmMode : byte
    {
        Off = 0,
        On = 1,
        Read = 0,
        Write = 0b1000_0000
    }
}