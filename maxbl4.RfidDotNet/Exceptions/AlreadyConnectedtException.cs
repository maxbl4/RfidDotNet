using System;

namespace maxbl4.RfidDotNet.Exceptions
{
    public class AlreadyConnectedtException : ApplicationException
    {
        public AlreadyConnectedtException()
        {
        }

        public AlreadyConnectedtException(string message) : base(message)
        {
        }

        public AlreadyConnectedtException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}