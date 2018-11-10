using System;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Ext;
using maxbl4.RfidDotNet.AlienTech.Interfaces;
using maxbl4.RfidDotNet.AlienTech.TagStream;
using Serilog;

namespace maxbl4.RfidDotNet.AlienTech
{
    public class ReconnectingAlienReaderProtocol : IDisposable
    {
        static readonly ILogger Logger = Log.ForContext<ReconnectingAlienReaderProtocol>();
        private readonly IPEndPoint endpoint;
        private readonly Func<IAlienReaderApi, Task> onConnected;
        private readonly int keepAliveTimeout;
        private readonly int receiveTimeout;
        private readonly bool usePolling;
        public bool AutoReconnect { get; set; }
        private Subject<Tag> tags = new Subject<Tag>();
        public IObservable<Tag> Tags => tags;
        private AlienReaderProtocol proto = null;
        public AlienReaderProtocol Current => proto;
        public int ReconnectTimeout { get; set; } = 2000;
        readonly SerialDisposable reconnectDisposable = new SerialDisposable();

        private readonly Subject<ConnectionStatus> connectionStatus = new Subject<ConnectionStatus>();
        public IObservable<ConnectionStatus> ConnectionStatus => connectionStatus;
        public bool IsConnected => proto?.IsConnected == true;
        
        public ReconnectingAlienReaderProtocol(IPEndPoint endpoint, Func<IAlienReaderApi, Task> onConnected,
            int keepAliveTimeout = AlienReaderProtocol.DefaultKeepaliveTimeout, 
            int receiveTimeout = AlienReaderProtocol.DefaultReceiveTimeout, bool usePolling = true)
        {
            this.endpoint = endpoint;
            this.onConnected = onConnected;
            this.keepAliveTimeout = keepAliveTimeout;
            this.receiveTimeout = receiveTimeout;
            this.usePolling = usePolling;
            var task = Connect();
        }

        private async Task Connect()
        {
            Logger.Information("Trying to connect to {endpoint}", endpoint);
            try
            {
                proto?.Dispose();
                var arp = new AlienReaderProtocol(keepAliveTimeout, receiveTimeout);
                await arp.ConnectAndLogin(endpoint.Address.ToString(), endpoint.Port, "alien", "password");
                if (usePolling)
                    await arp.StartTagPolling(tags);
                else
                    await arp.StartTagStreamOld(tags);
                arp.Disconnected += (s, e) => ScheduleReconnect(true);
                if (!arp.IsConnected) 
                    ScheduleReconnect();
                else
                {
                    proto = arp;
                    Logger.Swallow(() => connectionStatus.OnNext(new ConnectedEvent()));
                }
            }
            catch (Exception ex)
            {
                Logger.Warning("Could not connect to {endpoint} {ex}", endpoint, ex);
                ScheduleReconnect();
                Logger.Swallow(() => connectionStatus.OnNext(new FailedToConnect(ex)));
            }

            try
            {
                if (proto.IsConnected)
                    await onConnected(proto.Api);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "OnConnected handler failed {ex}");
                Logger.Swallow(() => connectionStatus.OnNext(new FailedToConnect(ex)));
            }
        }

        private void ScheduleReconnect(bool report = false)
        {
            if (report)
                Logger.Swallow(() => connectionStatus.OnNext(new DisconnectedEvent()));
            reconnectDisposable.Disposable = Observable.Timer(TimeSpan.FromMilliseconds(ReconnectTimeout))
                .Subscribe(x => Connect());
        }

        public void Dispose()
        {
            reconnectDisposable?.Dispose();
            Logger.Swallow(tags.OnCompleted);
            tags?.Dispose();
            proto?.Dispose();
        }
    }

    public class ConnectionStatus
    {
        public bool Connected { get; protected set; }
    }

    public class ConnectedEvent : ConnectionStatus
    {
        public ConnectedEvent()
        {
            Connected = true;
        }
    }

    public class DisconnectedEvent : ConnectionStatus
    {
        public DisconnectedEvent()
        {
            Connected = false;
        }
    }

    public class FailedToConnect : ConnectionStatus
    {
        public Exception Error { get; set; }

        public FailedToConnect(Exception error)
        {
            Connected = false;
            Error = error;
        }
    }
}