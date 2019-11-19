using System.IO;
using System.Net;
using System.Net.Sockets;
using maxbl4.Infrastructure.Extensions.DisposableExt;

namespace maxbl4.RfidDotNet.GenericSerial.DataAdapters
{
    public class NetworkStreamFactory : IDataStreamFactory
    {
        public const int DefaultTimeout = 2000;
        public DnsEndPoint EndPoint { get; }
        public int NetworkTimeout { get; }
        private NetworkStream stream = null;
        private Socket socket;

        public NetworkStreamFactory(DnsEndPoint endPoint, int networkTimeout = DefaultTimeout)
        {
            EndPoint = endPoint;
            NetworkTimeout = networkTimeout;
        }

        public Stream DataStream 
        {
            get
            {
                if (stream != null) return stream;
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = NetworkTimeout, ReceiveTimeout = NetworkTimeout
                };
                socket.Connect(EndPoint);
                stream = new NetworkStream(socket);
                return stream;
            }
        }

        public string Description => $"{EndPoint}";

        public void Invalidate()
        {
            stream.DisposeSafe();
            socket.DisposeSafe();
            stream = null;
            socket = null;
        }

        public void UpdateBaudRate(int baudRate)
        {
            throw new System.NotSupportedException();
        }

        public void Dispose()
        {
            Invalidate();
        }
    }
}