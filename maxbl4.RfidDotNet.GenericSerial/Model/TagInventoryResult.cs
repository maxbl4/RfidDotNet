using System;
using System.Collections.Generic;
using System.Text;
using maxbl4.RfidDotNet.GenericSerial.Exceptions;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using maxbl4.RfidDotNet.GenericSerial.Packets;

namespace maxbl4.RfidDotNet.GenericSerial.Model
{
    public class TagBufferResult
    {
        public List<Tag> Tags { get; } = new();
        public TagBufferResult(IEnumerable<ResponseDataPacket> packets)
        {
            foreach (var packet in packets)
            {
                if (packet.Command == ReaderCommand.GetTagsFromBuffer)
                {
                    switch (packet.Status)
                    {
                        case ResponseStatusCode.InventoryMoreFramesPending:
                        case ResponseStatusCode.InventoryComplete:
                            ReadBufferResult(packet);
                            continue;
                    }
                }
                throw new UnexpectedResponseException(packet.Command, packet.Status);
            }
        }
        
        void ReadBufferResult(ResponseDataPacket packet)
        {
            var offset = ResponseDataPacket.DataOffset;
            var epcIdCount = packet.RawData[offset++];
            for (var i = 0; i < epcIdCount; i++)
            {
                var tag = ReadEpcId(packet.RawData, ref offset);
                tag.LastSeenTime = tag.DiscoveryTime = packet.Timestamp;
                Tags.Add(tag);
            }
        }

        Tag ReadEpcId(byte[] buffer, ref int offset)
        {
            var antenna = ((GenAntennaConfiguration) buffer[offset++]).ToNumber();
            var epcLength = buffer[offset++];
            var epc = new StringBuilder(epcLength * 2);
            for (var i = 0; i < epcLength; i++)
            {
                epc.Append(buffer[offset++].ToString("X2"));
            }

            var rssi = buffer[offset++];
            var readCount = buffer[offset++];
            var tag = new Tag{TagId = epc.ToString(), Rssi = rssi, ReadCount = readCount, Antenna = antenna};
            return tag;
        }
    }

    public class TagInventoryResult
    {
        /// <summary>
        /// Cumulative time it took to receive all responses from reader.
        /// </summary>
        public TimeSpan Elapsed { get; private set; } = TimeSpan.Zero;
        /// <summary>
        /// Count of packets received from reader in response to TagInventory command
        /// </summary>
        public int PacketCount { get; private set; }
        /// <summary>
        /// Number of tags in buffer returned by TagInventoryWithMemoryBuffer command
        /// </summary>
        public ushort TagsInBuffer { get; private set; }
        /// <summary>
        /// Number of tags in last inventory returned by TagInventoryWithMemoryBuffer command
        /// </summary>
        public ushort TagsInLastInventory { get; private set;}
        public List<Tag> Tags { get; } = new();

        public TagInventoryResult(IEnumerable<ResponseDataPacket> packets)
        {
            foreach (var packet in packets)
            {
                RoutePacket(packet);
            }
        }

        void RoutePacket(ResponseDataPacket packet)
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
                            PacketCount++;
                            ReadInventoryResult(packet);
                            return;
                        case ResponseStatusCode.InventoryStatisticsDelivery:
                            ReadInventoryStatistics(packet);
                            return;
                    }
                    break;
                case ReaderCommand.TagInventoryWithMemoryBuffer:
                    switch (packet.Status)
                    {
                        case ResponseStatusCode.Success:
                            ReadBufferResponse(packet);
                            return;
                    }
                    break;
            }
            throw new UnexpectedResponseException(packet.Command, packet.Status);
        }

        void ReadBufferResponse(ResponseDataPacket packet)
        {
            Elapsed += packet.Elapsed;
            var offset = ResponseDataPacket.DataOffset;
            TagsInBuffer = (ushort)(packet.RawData[offset++] << 8);
            TagsInBuffer += packet.RawData[offset++];
            
            TagsInLastInventory = (ushort)(packet.RawData[offset++] << 8);
            TagsInLastInventory += packet.RawData[offset++];
        }

        void ReadInventoryStatistics(ResponseDataPacket packet)
        {
            
        }

        void ReadInventoryResult(ResponseDataPacket packet)
        {
            Elapsed += packet.Elapsed;
            var antenna = ((GenAntennaConfiguration) packet.RawData[ResponseDataPacket.DataOffset]).ToNumber();
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
    }
}