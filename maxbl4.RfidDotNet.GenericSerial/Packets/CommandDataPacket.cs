using System;
using maxbl4.RfidDotNet.GenericSerial.Model;

namespace maxbl4.RfidDotNet.GenericSerial.Packets
{
    public class CommandDataPacket
    {
        private const int BaseLength = 4;
        public byte Address { get; }
        public ReaderCommand Command { get; }
        public byte[] Data { get; }
        public int DataLength => Data?.Length ?? 0;
        public byte PacketLength => (byte) (BaseLength + DataLength);
        public int FullPacketLength => PacketLength + 1;

        public CommandDataPacket(ReaderCommand command, byte singleByteData, byte address = 0) 
            : this(command, new []{singleByteData}, address)
        {
        }

        public CommandDataPacket(ReaderCommand command, byte[] data = null, byte address = 0)
        {
            Command = command;
            Data = data;
            Address = address;
        }

        public byte[] Serialize()
        {
            var buf = new byte[FullPacketLength];
            buf[0] = PacketLength;
            buf[1] = Address;
            buf[2] = (byte)Command;
            if (Data != null) Array.Copy(Data, 0, buf, 3, DataLength);
            Crc16.SetCrc16(buf, FullPacketLength);
            return buf;
        }

        public static CommandDataPacket SetInventoryScanInterval(TimeSpan interval)
        {
            var t = (int)interval.TotalMilliseconds / 100;
            if (t < 0 || t > 255)
                throw new ArgumentOutOfRangeException(nameof(interval), $"Interval should be in 0 - 25500ms. Was {interval.TotalMilliseconds}");
            return new CommandDataPacket(ReaderCommand.SetInventoryScanInterval, (byte)t);
        }
        
        public static CommandDataPacket SetRFPower(byte rfPower)
        {
            return new CommandDataPacket(ReaderCommand.SetRFPower, rfPower);
        }
        
        public static CommandDataPacket GetNumberOfTagsInBuffer()
        {
            return new CommandDataPacket(ReaderCommand.GetNumberOfTagsInBuffer);
        }
        
        public static CommandDataPacket GetEpcLengthForBufferOperations()
        {
            return new CommandDataPacket(ReaderCommand.GetEpcLengthForBufferOperations);
        }
        
        public static CommandDataPacket SetEpcLengthForBufferOperations(EpcLength epcLength)
        {
            return new CommandDataPacket(ReaderCommand.SetEpcLengthForBufferOperations, (byte)epcLength);
        }
        
        public static CommandDataPacket SetAntennaConfiguration(AntennaConfiguration configuration)
        {
            return new CommandDataPacket(ReaderCommand.SetAntennaConfiguration, (byte)configuration);
        }
        
        public static CommandDataPacket SetAntennaCheck(bool enable)
        {
            return new CommandDataPacket(ReaderCommand.SetAntennaCheck, (byte)(enable ? 1 : 0));
        }
        
        public static CommandDataPacket TagInventory(ReaderCommand command, TagInventoryParams args)
        {
            return new CommandDataPacket(command, args.Serialize());
        }
        
        public static CommandDataPacket SetRealTimeInventoryParameters(RealtimeInventoryParams args)
        {
            return new CommandDataPacket(ReaderCommand.SetRealTimeInventoryParameters, args.Serialize());
        }
    }
}