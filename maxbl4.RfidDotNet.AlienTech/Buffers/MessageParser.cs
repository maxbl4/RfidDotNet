using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace maxbl4.RfidDotNet.AlienTech.Buffers
{
    public class MessageParser
    {
        public const int DefaultBufferSize = 5 * 1024 * 1024; // 5 Mb
        private readonly int bufferSize;
        public byte[] Buffer { get; }
        public int Offset { get; private set; }
        public int BufferLength => bufferSize - Offset;

        public MessageParser(int bufferSize = DefaultBufferSize)
        {
            this.bufferSize = bufferSize;
            Buffer = new byte[bufferSize];
        }

        public IEnumerable<string> Parse(int newDataLength, string terminatorChars = "\r\n\0")
        {
            if (string.IsNullOrEmpty(terminatorChars))
                throw new ArgumentException("Value cannot be null or empty.", nameof(terminatorChars));
            var terminators = terminatorChars.ToCharArray();

            if (newDataLength < 1) yield break;
            var end = newDataLength + Offset;
            int ind;
            var currentOffset = 0;
            while ((ind = FindTerminator(Buffer, currentOffset, end, terminators)) >= 0)
            {
                yield return Encoding.ASCII.GetString(Buffer, currentOffset, ind - currentOffset);
                while (ind < end && terminators.Any(x => x == Buffer[ind]))
                    ind++;
                currentOffset = ind;
            }
            if (currentOffset + 1 < end)
            {
                Array.Copy(Buffer, currentOffset, Buffer, 0, end - currentOffset);
                Offset = end - currentOffset;
            }
            else
                Offset = 0;

            if (BufferLength <= 0)
                throw new OutOfBufferSpace();
        }

        int FindTerminator(byte[] buffer, int offset, int end, char[] terminators)
        {
            for (int i = offset; i < end; i++)
            {
                if (terminators.Any(x => x == buffer[i]))
                    return i;
            }
            return -1;
        }
    }

    public class OutOfBufferSpace : ApplicationException
    {
    }
}