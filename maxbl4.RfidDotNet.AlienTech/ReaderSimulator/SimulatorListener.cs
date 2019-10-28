using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Ext;
using maxbl4.RfidDotNet.AlienTech.Net;
using maxbl4.RfidDotNet.Ext;
using Serilog;

namespace maxbl4.RfidDotNet.AlienTech.ReaderSimulator
{
    public class Simulator
    {
        public SimulatorProtocol Proto { get; set; }
        public SimulatorLogic Logic { get; set; }
    }

    public class SimulatorListener : IDisposable
    {
        static readonly ILogger Logger = Log.ForContext<SimulatorListener>();
        private readonly bool acceptSingleClient;
        private readonly TcpListener listener;
        private Simulator client;
        public const string ReaderAddress = "10.0.1.41";

        public Simulator Client => client;
        readonly List<Simulator> clients = new List<Simulator>();
        public TagStreamLogic TagStreamLogic { get; } = new TagStreamLogic();
        public Action<Socket> OnClientAccepted { get; set; }

        public SimulatorListener(IPEndPoint bindTo, bool acceptSingleClient = true)
        {
            this.acceptSingleClient = acceptSingleClient;
            listener = new TcpListener(bindTo);
            listener.Start();
            new Task(AcceptLoop, TaskCreationOptions.LongRunning).Start();
        }
        
        async void AcceptLoop()
        {
            try
            {
                while (true)
                {
                    var socket = await listener.AcceptSocketAsync();
                    lock (clients)
                    {
                        OnClientAccepted?.Invoke(socket);
                        Logger.Information("Accepted client {RemoteEndPoint}", socket.RemoteEndPoint);
                        if (acceptSingleClient && client?.Proto.IsConnected == true)
                        {
                            Logger.Information("Closing previous connection");
                            clients.Clear();
                            client.Proto.Dispose();
                            Logger.Information("Previous connection closed");
                        }

                        client = new Simulator {Logic = new SimulatorLogic()};
                        client.Proto = new SimulatorProtocol(client.Logic.HandleCommand);
                        clients.Add(client);
                        client.Proto.Accept(socket);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("SimulatorListener: {ex}", ex);
            }
        }
        
        public void Dispose()
        {
            listener?.Server.CloseForce();
            listener?.Stop();
            clients.ForEach(x => x?.Proto?.Dispose());
        }
    }
}