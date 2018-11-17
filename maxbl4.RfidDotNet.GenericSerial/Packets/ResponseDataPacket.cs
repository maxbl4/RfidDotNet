using System;
using System.Linq;
using System.Runtime.InteropServices;
using maxbl4.RfidDotNet.Exceptions;
using maxbl4.RfidDotNet.GenericSerial.Exceptions;
using maxbl4.RfidDotNet.GenericSerial.Model;

namespace maxbl4.RfidDotNet.GenericSerial.Packets
{
    public class ResponseDataPacket
    {
        public const byte HeaderLength = 5;
        public const int DataOffset = 4;
        public byte[] RawData { get; }
        public ReaderCommand ExpectedCommand { get; }
        public DateTimeOffset Timestamp { get; }
        
        /// <summary>
        /// Data.Length + 5
        /// </summary>
        public byte Length => RawData[0];
        public byte Address => RawData[1];
        public ReaderCommand Command => (ReaderCommand)RawData[2];
        public ResponseStatusCode Status => (ResponseStatusCode)RawData[3];
        public byte DataLength => (byte)(Length - HeaderLength);

        public ResponseDataPacket(ReaderCommand expectedCommand, byte[] rawData, DateTimeOffset? timestamp = null)
        {
            ExpectedCommand = expectedCommand;
            RawData = rawData;
            Timestamp = timestamp ?? DateTimeOffset.Now;
        }
        
        public Model.ReaderInfo GetReaderInfo()
        {
            ValidatePacket(12);
            return new Model.ReaderInfo(RawData, DataOffset);
        }

        public uint GetReaderSerialNumber()
        {
            ValidatePacket(4);
            return ReadUInt32();
        }

        uint ReadUInt32(int offset = DataOffset)
        {
            uint result = 0;
            result += (uint)(RawData[offset] << 24);
            result += (uint)(RawData[offset + 1] << 16);
            result += (uint)(RawData[offset + 2] << 8);
            result += RawData[offset + 3];
            return result;
        }

        void ValidatePacket(int expectedDataLength = -1)
        {
            if (Command != ExpectedCommand)
                throw new InvalidOperationException($"Wrong command {Command} != {ExpectedCommand}");
            if (expectedDataLength >= 0 && DataLength != expectedDataLength)
                throw new MalformedPacketException();
        }
        
        private static readonly ResponseStatusCode[] InventoryValidStatusCodes = 
        {
            ResponseStatusCode.InventoryComplete,
            ResponseStatusCode.InventoryTimeout,
            ResponseStatusCode.InventoryMoreFramesPending,
            ResponseStatusCode.InventoryBufferOverflow,
            ResponseStatusCode.InventoryStatisticsDelivery
        };

        public void CheckSuccess()
        {
            ValidatePacket(0);
            switch (Command)
            {
                case ReaderCommand.TagInventory:
                    if (!InventoryValidStatusCodes.Contains(Status)) return;
                    break;
                default:
                    if (Status == ResponseStatusCode.Success) return;
                    break;
            }
            throw new CommandExecutionFailedException(Command, Status);
        }
    }
}