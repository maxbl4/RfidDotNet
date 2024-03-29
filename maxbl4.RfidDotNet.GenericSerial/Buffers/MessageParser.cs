using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.ByteArrayExt;
using maxbl4.RfidDotNet.GenericSerial.Model;
using maxbl4.RfidDotNet.GenericSerial.Packets;
using Serilog;

namespace maxbl4.RfidDotNet.GenericSerial.Buffers
{
    public class MessageParser
    {
        private static readonly ILogger Logger = Log.ForContext<MessageParser>();
        public static async Task<PacketResult> ReadPacket(Stream stream, Stopwatch sw = null)
        {
            Logger.Debug("ReadPacket read packet length (1 byte)");
            if (sw == null)
                sw = Stopwatch.StartNew();
            var smallBuf = new byte[1];
            var read = stream.Read(smallBuf, 0, smallBuf.Length);
            var packetLength = smallBuf[0];
            Logger.Debug("ReadPacket packetLength={packetLength}", packetLength);
            if (read < 1)
            {
                Logger.Debug("ReadPacket Could not read packet length, return Timeout");
                return PacketResult.Timeout();
            }
            var totalRead = 0;
            var data = new byte[packetLength + 1];
            data[0] = packetLength;
            while (totalRead < packetLength)
            {
                read = stream.Read(data, totalRead + 1, packetLength - totalRead);
                if (read == 0)
                {
                    Logger.Debug("ReadPacket Could not complete reading of packet");
                    return PacketResult.WrongSize();
                }

                totalRead += read;
            }
            sw.Stop();

            if (!Crc16.CheckCrc16(data))
            {
                Logger.Debug("ReadPacket CRC check failed.");
                return PacketResult.WrongCrc();
            }
            
            Logger.Debug($"ReadPacket success: {data.ToHexString(" ")}");
            return PacketResult.FromData(data, sw.Elapsed);
        }

        public static bool ShouldReadMore(ResponseDataPacket responseDataPacket)
        {
            bool response = false;
            switch (responseDataPacket.Command)
            {
                case ReaderCommand.TagInventory:
                    response = responseDataPacket.Status == ResponseStatusCode.InventoryMoreFramesPending
                               || responseDataPacket.Status == ResponseStatusCode.InventoryStatisticsDelivery;
                    break;
                case ReaderCommand.RealtimeInventoryResponse:
                    response = true;
                    break;
            }
            Logger.Debug("{ShouldReadMore}", response);
            return response;
        }
    }
}