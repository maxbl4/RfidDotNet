using System;
using maxbl4.RfidDotNet.GenericSerial.Model;

namespace maxbl4.RfidDotNet.GenericSerial.Exceptions
{
    public class IllegalCommandException: Exception
    {
        public ReaderCommand Command { get; }
        public ResponseStatusCode Status { get; }
        
        public IllegalCommandException(
            ReaderCommand command,
            ResponseStatusCode status
        ): base($"Command {command} is illegal for current reader")
        {
            Command = command;
            Status = status;
        }
    }
}