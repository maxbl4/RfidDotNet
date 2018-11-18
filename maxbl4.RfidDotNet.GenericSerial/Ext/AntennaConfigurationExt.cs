using System;
using maxbl4.RfidDotNet.GenericSerial.Model;

namespace maxbl4.RfidDotNet.GenericSerial.Ext
{
    public static class AntennaConfigurationExt
    {
        public static int ToNumber(this AntennaConfiguration config)
        {
            switch (config)
            {
                case AntennaConfiguration.Antenna1:
                    return 0;
                case AntennaConfiguration.Antenna2:
                    return 1;
                case AntennaConfiguration.Antenna3:
                    return 2;
                case AntennaConfiguration.Antenna4:
                    return 3;
                default:
                    return (int)config;
            }
        }
    }
}