using System.Net;

namespace maxbl4.RfidDotNet
{
    public class SerialEndpoint : EndPoint
    {
        public SerialEndpoint(string port, int baudRate)
        {
            Port = port;
            BaudRate = baudRate;
        }

        public string Port { get; }
        public int BaudRate { get; }
    }
}