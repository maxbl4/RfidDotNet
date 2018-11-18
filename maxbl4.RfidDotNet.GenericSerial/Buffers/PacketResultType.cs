namespace maxbl4.RfidDotNet.GenericSerial.Buffers
{
    public enum PacketResultType
    {
        Success,
        WrongSize,
        WrongCrc,
        Timeout
    }
}