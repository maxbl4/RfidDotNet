using System;
using System.Text;

namespace maxbl4.RfidDotNet.Exceptions
{
    public class MalformedPacketException : Exception
    {
        public byte[] RawData { get; } 
        public MalformedPacketException()
        {
            
        }
        
        public MalformedPacketException(string message, byte[] buffer = null): base(FormatMessage(message, buffer))
        {
            RawData = buffer;
        }

        static string FormatMessage(string message, byte[] buffer)
        {
            if (buffer == null) return message;
            var sb = new StringBuilder(buffer.Length * 2);
            foreach (var t in buffer)
            {
                sb.Append(t.ToString("X2"));
            }
            return $"{message}{Environment.NewLine}{sb}";
        }
    }
}