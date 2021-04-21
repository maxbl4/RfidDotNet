using System;

namespace maxbl4.RfidDotNet.GenericSerial.Buffers
{
    public class PacketResult
    {
        public byte[] Data { get; set; }
        public bool Success => ResultType == PacketResultType.Success;
        public PacketResultType ResultType { get; set; }
        public TimeSpan Elapsed { get; set; }

        public static PacketResult FromData(byte[] data, TimeSpan elapsed) 
            => new() { Data = data, ResultType = PacketResultType.Success, Elapsed = elapsed};
        public static PacketResult WrongSize() 
            => new() { ResultType = PacketResultType.WrongSize};
        public static PacketResult WrongCrc() 
            => new() { ResultType = PacketResultType.WrongCrc};
        public static PacketResult Timeout() 
            => new() { ResultType = PacketResultType.Timeout};
    }
}