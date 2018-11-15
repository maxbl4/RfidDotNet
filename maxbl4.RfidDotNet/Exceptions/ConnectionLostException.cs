using System;

namespace maxbl4.RfidDotNet.Exceptions
{
    public class ConnectionLostException : ApplicationException
    {
        public ConnectionLostException()
        {
        }

        public ConnectionLostException(string message) : base(message)
        {
        }

        public ConnectionLostException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}