using System;

namespace maxbl4.RfidDotNet.AlienTech.Net
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