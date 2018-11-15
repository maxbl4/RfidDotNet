﻿using System;

namespace maxbl4.RfidDotNet.GenericSerial.Packets
{
    public class CommandDataPacket
    {
        private const int BaseLength = 4;
        public byte Address { get; }
        public ReaderCommand Command { get; }
        public byte[] Data { get; }

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
    }
}