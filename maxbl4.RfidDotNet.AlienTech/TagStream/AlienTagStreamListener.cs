using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Ext;
using maxbl4.RfidDotNet.Ext;
using Serilog;

namespace maxbl4.RfidDotNet.AlienTech.TagStream
{
    public class AlienTagStreamListener : IDisposable
    {
        static readonly ILogger Logger = Log.ForContext<AlienTagStreamListener>();
        readonly Subject<string> unparsedMessages = new Subject<string>();
        public IObservable<string> UnparsedMessages => unparsedMessages;
        readonly IObserver<Tag> tags;
        private readonly TcpListener tcpListener;
        readonly List<AlienTagStreamProtocol> connectedStreams = new List<AlienTagStreamProtocol>();
        private bool disposed = false;
        public IPEndPoint EndPoint { get; }

        public AlienTagStreamListener(IPEndPoint bindTo, IObserver<Tag> tags)
        {
            this.tags = tags;
            tcpListener = new TcpListener(bindTo);
            tcpListener.Start();
            EndPoint = (IPEndPoint)tcpListener.LocalEndpoint;
            new Task(AcceptLoop, TaskCreationOptions.LongRunning).Start();
        }

        private void AcceptLoop()
        {
            try
            {
                while (true)
                {
                    var client = tcpListener.AcceptSocket();
                    Logger.Debug("Accepted client {RemoteEndPoint}", client.RemoteEndPoint);
                    lock (connectedStreams)
                    {
                        if (disposed) return;
                        var tagReader = new AlienTagStreamProtocol(tags, unparsedMessages);
                        tagReader.Accept(client);
                        connectedStreams.Add(tagReader);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Warning(e, "AcceptLoop failure {e}");
                Dispose();
            }
        }

        public void Dispose()
        {
            lock (connectedStreams)
            {
                if (disposed) return;
                disposed = true;
                Logger.Swallow(() => unparsedMessages?.OnCompleted());
                unparsedMessages.DisposeSafe();
            }
        }
    }
}