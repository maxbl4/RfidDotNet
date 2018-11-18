using System;

namespace maxbl4.RfidDotNet.Exceptions
{
    public class ReceiveFailedException : Exception
    {
        public ReceiveFailedException()
        {
        }

        public ReceiveFailedException(string message): base(message)
        {
            
        }
    }
}