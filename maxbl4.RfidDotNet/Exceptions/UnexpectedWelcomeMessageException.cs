using System;
using System.Collections.Generic;

namespace maxbl4.RfidDotNet.Exceptions
{
    public class UnexpectedWelcomeMessageException : ApplicationException
    {
        public UnexpectedWelcomeMessageException(List<string> msgs): base($"Actual messages: {string.Join("\r\n", msgs)}")
        {
        }
    }
}