using System;

namespace maxbl4.RfidDotNet.Exceptions
{
    public class LoginFailedException : ApplicationException
    {
        public LoginFailedException(string response) : base(response)
        {
        }
    }
}