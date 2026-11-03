using System;
using System.IO;

namespace maxbl4.RfidDotNet.GenericSerial.Sl130
{
    public readonly record struct Sl130WorkMode(
        byte ReadMode,
        byte ModeState,
        byte MemInven,
        byte FirstAdr,
        byte WordNum,
        byte TagTime);

    public readonly record struct Sl130ReaderInfo(byte RfPower);

    public static class Sl130Protocol
    {
        public const byte GetReaderInformation = 0x21;
        public const byte SetRfPower = 0x2F;
        public const byte SetWorkMode = 0x35;
        public const byte GetWorkMode = 0x36;
        public const byte SetInventoryParameters = 0x3D;
        public const byte TagReport = 0xEE;

        public const byte ScanMode = 0x01;
        public const byte InventoryMultiple = 0x04;
        public const byte Rs232Output = 0x02;
        public const byte BeepOff = 0x04;

        public static byte[] BuildCommand(byte command, params byte[] data)
        {
            var length = 4 + data.Length;
            var packet = new byte[length + 1];
            packet[0] = (byte)length;
            packet[1] = 0;
            packet[2] = command;
            Array.Copy(data, 0, packet, 3, data.Length);
            Crc16.SetCrc16(packet, packet.Length);
            return packet;
        }

        public static Sl130WorkMode ForContinuousInventory(Sl130WorkMode current) => current with
        {
            ReadMode = ScanMode,
            ModeState = (byte)(current.ModeState | Rs232Output | BeepOff),
            MemInven = InventoryMultiple,
        };

        public static Sl130WorkMode ParseWorkMode(byte[] frame)
        {
            ValidateResponse(frame, GetWorkMode, 18);
            return new Sl130WorkMode(frame[8], frame[9], frame[10], frame[11], frame[12], frame[13]);
        }

        public static byte[] SerializeWorkMode(Sl130WorkMode mode) => new[]
        {
            mode.ReadMode, mode.ModeState, mode.MemInven, mode.FirstAdr, mode.WordNum, mode.TagTime,
        };

        public static Sl130ReaderInfo ParseReaderInfo(byte[] frame)
        {
            ValidateResponse(frame, GetReaderInformation, 14);
            return new Sl130ReaderInfo(frame[10]);
        }

        public static bool TryCreateTag(byte[] frame, DateTime timestampUtc, out Tag tag)
        {
            tag = null;
            if (!IsValidFrame(frame) || frame[2] != TagReport || frame[3] != 0 || frame.Length <= 6)
                return false;

            var epcLength = frame.Length - 6;
            tag = new Tag
            {
                TagId = Convert.ToHexString(frame, 4, epcLength),
                DiscoveryTime = timestampUtc,
                LastSeenTime = timestampUtc,
                Antenna = 0,
                ReadCount = 1,
                Rssi = 0,
            };
            return true;
        }

        public static void ValidateResponse(byte[] frame, byte expectedCommand, int? expectedLength = null)
        {
            if (!IsValidFrame(frame))
                throw new InvalidDataException("SL130 returned a malformed frame or invalid CRC");
            if (expectedLength.HasValue && frame.Length != expectedLength.Value)
                throw new InvalidDataException(
                    $"SL130 command 0x{expectedCommand:X2} returned {frame.Length} bytes, expected {expectedLength.Value}");
            if (frame[2] != expectedCommand)
                throw new InvalidDataException(
                    $"SL130 response command 0x{frame[2]:X2} does not match 0x{expectedCommand:X2}");
            if (frame[3] != 0)
                throw new InvalidDataException(
                    $"SL130 command 0x{expectedCommand:X2} failed with status 0x{frame[3]:X2}");
        }

        private static bool IsValidFrame(byte[] frame) =>
            frame is { Length: >= 6 } && frame[0] + 1 == frame.Length && Crc16.CheckCrc16(frame);
    }
}
