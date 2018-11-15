using System;

namespace maxbl4.RfidDotNet.GenericSerial.Packets
{
    public class ResponseDataPacket
    {
        /// <summary>
        /// Data.Length + 5
        /// </summary>
        public byte Length { get; set; }
        public byte Address { get; set; }
        public byte ReCmd { get; set; }
        public ReaderCommand Command { get; set; }
        public byte[] Data { get; set; }
        public UInt16 Crc { get; set; }
    }
}