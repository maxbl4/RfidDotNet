using System;
using System.Linq;
using System.Text;
using maxbl4.RfidDotNet.Exceptions;
using maxbl4.RfidDotNet.GenericSerial.Exceptions;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using maxbl4.RfidDotNet.GenericSerial.Model;

namespace maxbl4.RfidDotNet.GenericSerial.Packets
{
    public class ResponseDataPacket
    {
        private readonly IObserver<Exception> errorsObserver;
        public const byte HeaderLength = 5;
        public const int DataOffset = 4;
        public byte[] RawData { get; }
        public ReaderCommand ExpectedCommand { get; }
        public DateTime Timestamp { get; } = new(0, DateTimeKind.Utc);
        public TimeSpan Elapsed { get; }
        
        /// <summary>
        /// Data.Length + 5
        /// </summary>
        public byte Length => RawData[0];
        public byte Address => RawData[1];
        public ReaderCommand Command => (ReaderCommand)RawData[2];
        public ResponseStatusCode Status => (ResponseStatusCode)RawData[3];
        public byte DataLength => (byte)(Length - HeaderLength);

        public ResponseDataPacket(ReaderCommand expectedCommand, byte[] rawData, DateTime? timestamp = null, 
            TimeSpan? elapsed = null, IObserver<Exception> errorsObserver = null)
        {
            this.errorsObserver = errorsObserver;
            ExpectedCommand = expectedCommand;
            RawData = rawData;
            Elapsed = elapsed ?? TimeSpan.Zero;
            Timestamp = timestamp ?? DateTime.UtcNow;
        }
        
        public Model.ReaderInfo GetReaderInfo()
        {
            if (!ValidatePacket(12)) return null;
            return new Model.ReaderInfo(RawData, DataOffset);
        }

        public uint GetReaderSerialNumber()
        {
            if (!ValidatePacket(4)) return 0;
            return ReadUInt32();
        }
        
        public int GetReaderTemperature()
        {
            if (!ValidatePacket(2)) return 0;
            var offset = DataOffset;
            var positive = RawData[offset++] > 0;
            var temp = RawData[offset++] * (positive ? 1 : -1);
            return temp;
        }
        
        public DrmMode GetDrmEnabled()
        {
            if (!ValidatePacket(1)) return DrmMode.Off;
            return (DrmMode)RawData[DataOffset];
        }
        
        public Tag GetRealtimeTag(out bool isHeartbeat)
        {
            const int baseDataLength = 3;
            ValidatePacket(minimumDataLength: baseDataLength);
            isHeartbeat = Status == ResponseStatusCode.HeartBeatDelivered;
            if (isHeartbeat) return null;
            if (Status != ResponseStatusCode.Success)
                throw new MalformedPacketException($"Got realtime tag report with unexpected status {Status}, " +
                                                   $"expected {ResponseStatusCode.Success}", RawData);

            var offset = DataOffset;
            var ant = ((GenAntennaConfiguration)RawData[offset++]).ToNumber();
            var epcLength = RawData[offset++];
            if (DataLength != baseDataLength + epcLength)
                throw new MalformedPacketException($"Got realtime tag report with inconsistent epc length {epcLength}, " +
                                                   $"expected {DataLength - baseDataLength}", RawData);
            var epc = new StringBuilder(epcLength * 2);
            for (var i = 0; i < epcLength; i++)
            {
                epc.Append(RawData[offset++].ToString("X2"));
            }
            var rssi = RawData[offset++];
            return new Tag{Antenna = ant, TagId = epc.ToString(), Rssi = rssi, LastSeenTime = Timestamp, DiscoveryTime = Timestamp, ReadCount = 1};
        }
        
        public EpcLength GetEpcLength()
        {
            if (!ValidatePacket(1)) return EpcLength.UpTo128Bits;
            return (EpcLength)RawData[DataOffset];
        }
        
        public int GetNumberOfTagsInBuffer()
        {
            if (!ValidatePacket(2)) return 0;
            return ReadUInt16();
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
        
        ushort ReadUInt16(int offset = DataOffset)
        {
            ushort result = 0;
            result += (ushort)(RawData[offset] << 8);
            result += RawData[offset + 1];
            return result;
        }

        bool ValidatePacket(int expectedDataLength = -1, int minimumDataLength = -1)
        {
            if (Status == ResponseStatusCode.IllegalCommand)
            {
                if (errorsObserver == null) 
                    throw new IllegalCommandException(Command, Status);
                errorsObserver.OnNext(new IllegalCommandException(Command, Status));
                return false;
            }

            if (Command != ExpectedCommand)
                throw new InvalidOperationException($"Wrong command {Command} != {ExpectedCommand}");
            if (expectedDataLength >= 0 && DataLength != expectedDataLength)
                throw new MalformedPacketException($"Got packet with unexpected length {DataLength}, expected {expectedDataLength}", RawData);
            if (minimumDataLength >= 0 && DataLength < minimumDataLength)
                throw new MalformedPacketException($"Got packet with data length less then expected {DataLength} < {minimumDataLength}", RawData);
            return true;
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
            if (!ValidatePacket(0)) return;
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