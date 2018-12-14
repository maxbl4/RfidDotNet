using System;

namespace maxbl4.RfidDotNet
{
    [Flags]
    public enum AntennaConfiguration: byte
    {
        Nothing = 0,
        Antenna1 = 0b0001,
        Antenna2 = 0b0010,
        Antenna3 = 0b0100,
        Antenna4 = 0b1000,
    }
}