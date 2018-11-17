using System;
using maxbl4.RfidDotNet.GenericSerial.Model;

namespace maxbl4.RfidDotNet.GenericSerial.Exceptions
{
    public class CommandExecutionFailedException: Exception
    {
        public ReaderCommand Command { get; }
        public ResponseStatusCode Status { get; }
        
        public CommandExecutionFailedException(
            ReaderCommand command,
            ResponseStatusCode status
            ): base($"Response for command {command} returned non success status {status}")
        {
            Command = command;
            Status = status;
        }
    }
}