using System;
using maxbl4.RfidDotNet.GenericSerial.Model;

namespace maxbl4.RfidDotNet.GenericSerial.Exceptions
{
    public class UnexpectedResponseException : Exception
    {
        public ReaderCommand Command { get; }
        public ResponseStatusCode Status { get; }
        
        public UnexpectedResponseException(
            ReaderCommand command,
            ResponseStatusCode status
        ): base($"Response for command {command} returned unexpected status {status}")
        {
            Command = command;
            Status = status;
        }
    }
}