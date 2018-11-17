using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.Exceptions;
using maxbl4.RfidDotNet.GenericSerial.Model;
using maxbl4.RfidDotNet.GenericSerial.Packets;

namespace maxbl4.RfidDotNet.GenericSerial.Buffers
{
    public class MessageParser
    {
        public static async Task<PacketResult> ReadPacket(Stream stream)
        {
            var packetLength = stream.ReadByte();
            if (packetLength < 0) return PacketResult.WrongSize();
            var totalRead = 0;
            var data = new byte[packetLength + 1];
            data[0] = (byte)packetLength;
            while (totalRead < packetLength)
            {
                totalRead += await stream.ReadAsync(data, totalRead + 1, packetLength - totalRead);
            }
            if (!Crc16.CheckCrc16(data)) 
                return PacketResult.WrongCrc();
            return PacketResult.FromData(data);
        }

        public static bool ShouldReadMore(ResponseDataPacket responseDataPacket)
        {
            if (responseDataPacket.Command == ReaderCommand.TagInventory
                && (responseDataPacket.Status == ResponseStatusCode.InventoryMoreFramesPending
                    || responseDataPacket.Status == ResponseStatusCode.InventoryStatisticsDelivery))
                return true;
            return false;
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