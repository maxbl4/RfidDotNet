using System;
using maxbl4.RfidDotNet.GenericSerial.Model;

namespace maxbl4.RfidDotNet.GenericSerial.Ext
{
    public static class BaudRateExt
    {
        public static int ToNumber(this BaudRates baud)
        {
            switch (baud)
            {
                case BaudRates.Baud9600:
                    return 9600;
                case BaudRates.Baud19200:
                    return 19200;
                case BaudRates.Baud38400:
                    return 38400;
                case BaudRates.Baud57600:
                    return 57600;
                case BaudRates.Baud115200:
                    return 115200;
                default:
                    throw new ArgumentOutOfRangeException(nameof(baud), baud, null);
            }
        }
    }
}