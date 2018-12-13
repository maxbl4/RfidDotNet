using System;
using maxbl4.RfidDotNet.GenericSerial.Model;

namespace maxbl4.RfidDotNet.GenericSerial.Ext
{
    public static class AntennaConfigurationExt
    {
        public static int ToNumber(this GenAntennaConfiguration config)
        {
            switch (config)
            {
                case GenAntennaConfiguration.Antenna1:
                    return 0;
                case GenAntennaConfiguration.Antenna2:
                    return 1;
                case GenAntennaConfiguration.Antenna3:
                    return 2;
                case GenAntennaConfiguration.Antenna4:
                    return 3;
                default:
                    return (int)config;
            }
        }
    }
}