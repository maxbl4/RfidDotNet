using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Net;

namespace maxbl4.RfidDotNet.AlienTech.ReaderSimulator
{
    public class TagStreamSimulatorProtocol : DuplexProtocol
    {
        public TagStreamSimulatorProtocol(int receiveTimeout = AlienReaderProtocol.DefaultReceiveTimeout) : base(receiveTimeout) { }

        public void Accept(Socket client)
        {
            if (client?.Connected != true)
                throw new ArgumentException("Socket should be connected", nameof(client));
            Connect(client);
            new Task(ReceiveLoop, TaskCreationOptions.LongRunning).Start();
        }

        private void ReceiveLoop()
        {
            
        }

        public override string IncomingMessageTerminators => "\0";

        public Task Send(string data)
        {
            return SendRaw(data + "\r\n\0");
        }
    }
}