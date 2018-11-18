namespace maxbl4.RfidDotNet.GenericSerial.Buffers
{
    public class PacketResult
    {
        public byte[] Data { get; set; }
        public bool Success => ResultType == PacketResultType.Success;
        public PacketResultType ResultType { get; set; }

        public static PacketResult FromData(byte[] data) 
            => new PacketResult { Data = data, ResultType = PacketResultType.Success};
        public static PacketResult WrongSize() 
            => new PacketResult { ResultType = PacketResultType.WrongSize};
        public static PacketResult WrongCrc() 
            => new PacketResult { ResultType = PacketResultType.WrongCrc};
        public static PacketResult Timeout() 
            => new PacketResult { ResultType = PacketResultType.Timeout};
    }
}