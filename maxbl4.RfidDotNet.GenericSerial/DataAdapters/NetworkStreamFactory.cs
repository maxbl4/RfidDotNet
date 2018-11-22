using System.IO;
using System.Net;
using System.Net.Sockets;
using maxbl4.RfidDotNet.GenericSerial.Ext;

namespace maxbl4.RfidDotNet.GenericSerial.DataAdapters
{
    public class NetworkStreamFactory : IDataStreamFactory
    {
        public const int DefaultTimeout = 2000;
        public string Hostname { get; }
        public int Port { get; }
        public int NetworkTimeout { get; }
        private NetworkStream stream = null;
        private Socket socket;

        public NetworkStreamFactory(string hostname, int port, int networkTimeout = DefaultTimeout)
        {
            Hostname = hostname;
            Port = port;
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
                socket.Connect(Hostname, Port);
                stream = new NetworkStream(socket);
                return stream;
            }
        }

        public string Description => $"{Hostname}:{Port}";

        public void Invalidate()
        {
            stream.DisposeSafe();
            socket.DisposeSafe();
            stream = null;
            socket = null;
        }

        public void Dispose()
        {
            Invalidate();
        }
    }
}