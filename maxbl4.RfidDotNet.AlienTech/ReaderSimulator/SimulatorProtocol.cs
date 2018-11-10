using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Net;
using Serilog;

namespace maxbl4.RfidDotNet.AlienTech.ReaderSimulator
{
    public class SimulatorProtocol : DuplexProtocol
    {
        static readonly ILogger Logger = Log.ForContext<SimulatorProtocol>();
        private readonly Func<string, string> commandHandler;
        public override string IncomingMessageTerminators => "\r\n";

        public SimulatorProtocol(Func<string,string> commandHandler) : base(int.MaxValue)
        {
            this.commandHandler = commandHandler;
        }

        public void Accept(Socket client)
        {
            if (client?.Connected != true)
                throw new ArgumentException("Socket should be connected", nameof(client));
            Connect(client);
            new Task(RecieveLoop, TaskCreationOptions.LongRunning).Start();
        }

        private async void RecieveLoop()
        {
            try
            {
                var response = await SendRecieve(ProtocolMessages.Welcome, ">");
                while (true)
                {
                    var data = commandHandler(response);
                    response = await SendRecieve(data ?? "", data == null ? "" : "\0");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Dispose();
            }
        }
        
        public async Task<string> SendRecieve(string data, string terminator = "\0")
        {
            return string.Join(IncomingMessageTerminators, await SendReceiveRaw(data + terminator))
                .TrimEnd('\r', '\n');
        }
    }
}