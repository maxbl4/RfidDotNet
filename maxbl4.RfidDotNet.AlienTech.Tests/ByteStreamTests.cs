using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reactive.Linq;
using maxbl4.RfidDotNet.AlienTech.Ext;
using maxbl4.RfidDotNet.AlienTech.Net;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests
{
    public class ByteStreamTests
    {
        [Fact]
        public void Timeout_should_throw()
        {
            using (var con = new SocketConnection())
            {
                Assert.Throws<ConnectionLostException>(() => con.ClientStream.Read());
                con.Client.Connected.ShouldBeFalse();
            }
        }

        [Fact]
        public void Timeout_after_some_data_should_throw()
        {
            using (var con = new SocketConnection())
            {
                con.ServerStream.Send("hello\0");
                con.ClientStream.Read()[0].ShouldBe("hello");
                Assert.Throws<ConnectionLostException>(() => con.ClientStream.Read());
                con.Client.Connected.ShouldBeFalse();
            }
        }

        [Fact]
        public void Socket_connect_timeout()
        {
            for (int i = 0; i < 5; i++)
            {
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                var sw = Stopwatch.StartNew();
                Observable.Timer(DateTime.Now.AddMilliseconds(500))
                    .Subscribe(x => socket.CloseForce());
                Assert.ThrowsAny<Exception>(() => socket.Connect("10.0.0.253", 25));
                sw.Stop();
                sw.ElapsedMilliseconds.ShouldBeLessThan(4000);
            }
        }
    }

    class SocketConnection : IDisposable
    {
        public TcpListener Listener { get; }
        public Socket Server { get; }
        public Socket Client { get; }
        public ByteStream ClientStream { get; }
        public ByteStream ServerStream { get; }

        public SocketConnection()
        {
            Listener = new TcpListener(GetDefaultIpAddress(), 44444);
            Listener.Start();
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Client.Connect(GetDefaultIpAddress(), 44444);
            Server = Listener.AcceptSocket();
            ServerStream = new ByteStream(Server, 100);
            ClientStream = new ByteStream(Client, 100);
        }
        
        public void Dispose()
        {
            Client.Close();
            Server.Close();
            Listener.Stop();
        }

        public static IPAddress GetDefaultIpAddress()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(x => x.OperationalStatus == OperationalStatus.Up && x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                .Select(x => x.Address)
                .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}