using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Ext;
using maxbl4.RfidDotNet.AlienTech.Interfaces;
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
        private object sync = new object();

        public static readonly Func<string> DefaultTagListHandler = () => ProtocolMessages.NoTags;

        public Func<string> tagListHandler = DefaultTagListHandler;
        public Func<string> TagListHandler
        {
            get => tagListHandler;
            set
            {
                tagListHandler = value;
                if (Client?.Logic != null)
                    Client.Logic.TagListHandler = value;
            }
        }

        public Simulator Client => client;

        public IPEndPoint ListenEndpoint => (IPEndPoint)listener.LocalEndpoint;
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
                    lock (sync)
                    {
                        OnClientAccepted?.Invoke(socket);
                        Logger.Information("Accepted client {RemoteEndPoint}", socket.RemoteEndPoint);
                        if (acceptSingleClient && client?.Proto.IsConnected == true)
                        {
                            Logger.Information("Closing previous connection");
                            client.Proto.DisposeSafe();
                            Logger.Information("Previous connection closed");
                        }

                        client = new Simulator {Logic = new SimulatorLogic{TagListHandler = TagListHandler}};
                        client.Proto = new SimulatorProtocol(client.Logic.HandleCommand);
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
        }
    }
}