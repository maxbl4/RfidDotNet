namespace maxbl4.RfidDotNet.GenericSerial.Model
{
    public enum ReaderWorkingMode : byte
    {
        Answer = 0x0,
        Realtime = 0x1,
        RealtimeGPIOTriggered = 0x2
    }
}