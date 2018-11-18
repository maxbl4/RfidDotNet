using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.Exceptions;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using maxbl4.RfidDotNet.GenericSerial.Model;
using maxbl4.RfidDotNet.GenericSerial.Packets;
using Serilog;

namespace maxbl4.RfidDotNet.GenericSerial.Buffers
{
    public class MessageParser
    {
        private static readonly ILogger Logger = Log.ForContext<MessageParser>();
        public static async Task<PacketResult> ReadPacket(Stream stream)
        {
            Logger.Debug("ReadPacket read packet length (1 byte)");
            var packetLength = stream.ReadByte();
            Logger.Debug("ReadPacket packetLength={packetLength}", packetLength);
            if (packetLength < 0)
            {
                Logger.Debug("ReadPacket Could not read packet length, return Timeout");
                return PacketResult.Timeout();
            }
            var totalRead = 0;
            var data = new byte[packetLength + 1];
            data[0] = (byte)packetLength;
            while (totalRead < packetLength)
            {
                var read = await stream.ReadAsync(data, totalRead + 1, packetLength - totalRead);
                if (read == 0)
                {
                    Logger.Debug("ReadPacket Could not complete reading of packet");
                    return PacketResult.WrongSize();
                }

                totalRead += read;
            }

            if (!Crc16.CheckCrc16(data))
            {
                Logger.Debug("ReadPacket CRC check failed.");
                return PacketResult.WrongCrc();
            }
            
            Logger.Debug($"ReadPacket success: {data.ToHexString(" ")}");
            return PacketResult.FromData(data);
        }

        public static bool ShouldReadMore(ResponseDataPacket responseDataPacket)
        {
            var response = responseDataPacket.Command == ReaderCommand.TagInventory
                && (responseDataPacket.Status == ResponseStatusCode.InventoryMoreFramesPending
                    || responseDataPacket.Status == ResponseStatusCode.InventoryStatisticsDelivery);
            Logger.Debug("{ShouldReadMore}", response);
            return response;
        }
    }
}