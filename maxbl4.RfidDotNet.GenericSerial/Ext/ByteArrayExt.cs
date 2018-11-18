using System.Linq;
using System.Text;

namespace maxbl4.RfidDotNet.GenericSerial.Ext
{
    public static class ByteArrayExt
    {
        public static string ToHexString(this byte[] data, string delimeter = "", int start = 0, int length = -1)
        {
            if (data == null) return "<null>";
            if (length < 0)
                length = data.Length;
            return string.Join(delimeter, data.Skip(start).Take(length).Select(x => x.ToString("X2")));
        }
    }
}