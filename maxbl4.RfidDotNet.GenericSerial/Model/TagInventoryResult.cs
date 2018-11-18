using System;
using System.Collections.Generic;
using System.Text;
using maxbl4.RfidDotNet.Exceptions;
using maxbl4.RfidDotNet.GenericSerial.Exceptions;
using maxbl4.RfidDotNet.GenericSerial.Packets;

namespace maxbl4.RfidDotNet.GenericSerial.Model
{
    public class TagInventoryResult
    {
        public ushort TagsInBuffer { get; private set; }
        public ushort TagsInLastInventory { get; private set;}
        public List<Tag> Tags { get; } = new List<Tag>();
        public TagInventoryResult(IEnumerable<ResponseDataPacket> packets)
        {
            foreach (var packet in packets)
            {
                switch (packet.Command)
                {
                    case ReaderCommand.TagInventory:
                        switch (packet.Status)
                        {
                            case ResponseStatusCode.InventoryTimeout:
                            case ResponseStatusCode.InventoryMoreFramesPending:
                            case ResponseStatusCode.InventoryBufferOverflow:
                            case ResponseStatusCode.InventoryComplete:
                                ReadInventoryResult(packet);
                                continue;
                            case ResponseStatusCode.InventoryStatisticsDelivery:
                                ReadInventoryStatistics(packet);
                                continue;
                        }
                        break;
                    case ReaderCommand.TagInventoryWithMemoryBuffer:
                        switch (packet.Status)
                        {
                            case ResponseStatusCode.Success:
                                ReadBufferResponse(packet);
                                continue;
                        }
                        break;
                }
                throw new UnexpectedResponseException(packet.Command, packet.Status);
            }
        }

        void ReadBufferResponse(ResponseDataPacket packet)
        {
            var offset = ResponseDataPacket.DataOffset;
            // Here our friends have violated the protocol spec. Most significant byte is returned first...
            TagsInBuffer = (ushort)(packet.RawData[offset++] << 8);
            TagsInBuffer += packet.RawData[offset++];
            
            TagsInLastInventory = (ushort)(packet.RawData[offset++] << 8);
            TagsInLastInventory += packet.RawData[offset++];
        }

        void ReadInventoryStatistics(ResponseDataPacket packet)
        {
            throw new NotImplementedException();
        }

        void ReadInventoryResult(ResponseDataPacket packet)
        {
            var antenna = AntennaConfigurationToNumber((AntennaConfiguration)packet.RawData[ResponseDataPacket.DataOffset]);
            var epcIdCount = packet.RawData[ResponseDataPacket.DataOffset + 1];
            var offset = ResponseDataPacket.DataOffset + 2;
            for (var i = 0; i < epcIdCount; i++)
            {
                var tag = ReadEpcId(packet.RawData, ref offset);
                tag.LastSeenTime = tag.DiscoveryTime = packet.Timestamp;
                tag.Antenna = antenna;
                Tags.Add(tag);
            }
        }

        Tag ReadEpcId(byte[] buffer, ref int offset)
        {
            var length = buffer[offset] & 0b0111_1111;
            var hasEpcPlusTid = (buffer[offset] & 0b1000_0000) > 0;
            var epcLength = hasEpcPlusTid ? length / 2 : length;
            var epc = new StringBuilder(epcLength * 2);
            for (var i = 0; i < epcLength; i++)
            {
                epc.Append(buffer[offset + 1 + i].ToString("X2"));
            }
            var tag = new Tag{TagId = epc.ToString(), Rssi = buffer[offset + length + 1], ReadCount = 1};
            offset += length + 2;
            return tag;
        }

        int AntennaConfigurationToNumber(AntennaConfiguration config)
        {
            switch (config)
            {
                case AntennaConfiguration.Antenna1:
                    return 0;
                case AntennaConfiguration.Antenna2:
                    return 1;
                case AntennaConfiguration.Antenna3:
                    return 2;
                case AntennaConfiguration.Antenna4:
                    return 3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(config), config, null);
            }
        }
    }
}