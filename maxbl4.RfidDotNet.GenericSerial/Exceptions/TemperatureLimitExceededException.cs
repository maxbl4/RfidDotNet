using System;

namespace maxbl4.RfidDotNet.GenericSerial.Exceptions
{
    public class TemperatureLimitExceededException : Exception
    {
        public TemperatureLimitExceededException(int temperatureLimit, int actualTemperature)
            :base($"Reader is overheating, temperature is {actualTemperature} while limit was set to {temperatureLimit}. To prevent damage, stopping inventory.")
        {
        }
    }

    public class TagReadBufferIsFull : Exception
    {
        
    }
}