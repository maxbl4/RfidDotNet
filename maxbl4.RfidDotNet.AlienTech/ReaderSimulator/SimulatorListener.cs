using System;
using System.Collections.Generic;
using System.IO;
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
        private static int basePort = 20000;
        private readonly TcpListener listener;
        private Simulator client;
        private readonly string host;
        public const string ReaderAddress = "192.168.1.100";

        public Simulator Client => client;
        List<Simulator> clients = new List<Simulator>();
        public bool UsePhysicalDevice { get; }
        public string Host => UsePhysicalDevice ? ReaderAddress : host;
        public int Port => UsePhysicalDevice ? 23 : basePort;
        public TagStreamLogic TagStreamLogic { get; } = new TagStreamLogic();


        public SimulatorListener(bool acceptSingleClient = true, bool? usePhysicalDevice = null)
        {
            basePort++;
            UsePhysicalDevice = usePhysicalDevice ?? File.Exists("AlienTests_UsePhysicalDevice");
            this.acceptSingleClient = acceptSingleClient;
            host = EndpointLookup.GetAnyPhysicalIp().ToString();
            if (UsePhysicalDevice) return;
            listener = new TcpListener(EndpointLookup.GetAnyPhysicalIp(), basePort);
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