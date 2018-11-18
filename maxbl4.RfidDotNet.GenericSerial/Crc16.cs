using System;

namespace maxbl4.RfidDotNet.GenericSerial
{
    public static class Crc16
    {
        private const ushort Polynomial = 0x8408;
        
        public static void SetCrc16(byte[] data, int length = -1)
        {
            if (length == -1) length = data.Length;
            if (data[0] + 1 != length)
                throw new ArgumentOutOfRangeException(nameof(data), "Malformed packet. First byte must specify length of data. And buffer size must be length of data + 1");
            ushort crcValue = 0xFFFF;
            var checkedDataLength = data[0] - 1;
            for (var i = 0; i < checkedDataLength; i++)
            {
                crcValue ^= data[i];
                for (var ucJ = 0; ucJ < 8; ucJ++)
                {
                    if ((crcValue & 0x01) != 0)
                        crcValue = (ushort) ((crcValue >> 1) ^ Polynomial);
                    else
                        crcValue = (ushort) (crcValue >> 1);
                }
            }
            data[checkedDataLength] = (byte) crcValue;
            data[checkedDataLength + 1] = (byte) (crcValue >> 8);
        }
        
        public static bool CheckCrc16(byte[] data, int length = -1)
        {
            if (length == -1) length = data.Length;
            if (data[0] + 1 != length) return false;
            ushort crcValue = 0xFFFF;
            foreach (var b in data)
            {
                crcValue ^= b;
                for (var j = 0; j < 8; j++)
                {
                    if ((crcValue & 0x01) != 0)
                        crcValue = (ushort)((crcValue >> 1) ^ Polynomial);
                    else
                        crcValue = (ushort)(crcValue >> 1);
                }
            }
            return crcValue == 0;
        }
    }
}