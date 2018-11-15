using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using maxbl4.RfidDotNet.Exceptions;

namespace maxbl4.RfidDotNet.GenericSerial.Buffers
{
    public class MessageParser
    {
        public PacketResult ReadPacket(Stream stream)
        {
            var packetLength = stream.ReadByte();
            if (packetLength < 0) return PacketResult.WrongSize();
            var totalRead = 0;
            var data = new byte[packetLength + 1];
            data[0] = (byte)packetLength;
            while (totalRead < packetLength)
            {
                totalRead += stream.Read(data, totalRead + 1, packetLength - totalRead);
            }
            if (!Crc16.CheckCrc16(data)) 
                return PacketResult.WrongCrc();
            return PacketResult.FromData(data);
        }
    }

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
    }

    public enum PacketResultType
    {
        Success,
        WrongSize,
        WrongCrc
    }
}