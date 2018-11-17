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

        public int Serialize(byte[] buf)
        {
            var dataLength = Data?.Length ?? 0;
            buf[0] = (byte)(BaseLength + dataLength);
            buf[1] = Address;
            buf[2] = (byte)Command;
            if (Data != null) Array.Copy(Data, 0, buf, 3, dataLength);
            var written = BaseLength + dataLength + 1;
            Crc16.SetCrc16(buf, written);
            return written;
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
        
        public static CommandDataPacket SetAntennaConfiguration(AntennaConfiguration configuration)
        {
            return new CommandDataPacket(ReaderCommand.SetAntennaConfiguration, (byte)configuration);
        }
        
        public static CommandDataPacket SetAntennaCheck(bool enable)
        {
            return new CommandDataPacket(ReaderCommand.SetAntennaCheck, (byte)(enable ? 1 : 0));
        }
    }
}