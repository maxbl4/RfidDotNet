using System;

namespace maxbl4.RfidDotNet.GenericSerial.Model
{
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