using System;

namespace maxbl4.RfidDotNet.GenericSerial.Model
{
    [Flags]
    public enum ProtocolType: byte
    {
        Gen18000_6B = 0b01,
        Gen18000_6C = 0b10,
    }
}