using System;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.RfidDotNet.AlienTech.Enums;
using maxbl4.RfidDotNet.AlienTech.Extensions;
using maxbl4.RfidDotNet.AlienTech.Interfaces;
using Serilog;

namespace maxbl4.RfidDotNet.AlienTech
{
    public class ReconnectingAlienReaderProtocol : IUniversalTagStream
    {
        private readonly ConnectionString connectionString;
        static readonly ILogger Logger = Log.ForContext<ReconnectingAlienReaderProtocol>();
        private readonly DnsEndPoint endpoint;
        private readonly Func<IAlienReaderApi, Task> onConnected;
        private readonly int keepAliveTimeout;
        private readonly int receiveTimeout;
        private readonly bool usePolling;
        public bool AutoReconnect { get; set; }
        private Subject<Tag> tags = new();
        public IObservable<Tag> Tags => tags;
        private Subject<Exception> errors = new();
        public IObservable<Exception> Errors => errors;
        private BehaviorSubject<bool> connected = new(false);
        public IObservable<bool> Connected => connected;
        private BehaviorSubject<DateTime> heartbeat = new(DateTime.MinValue);
        public IObservable<DateTime> Heartbeat => heartbeat;
        public Task Start()
        {
            return Connect();
        }

        public bool Start2()
        {
            return false;
        }

        public Task<int> QValue(int? newValue = null)
        {
            if (newValue != null) connectionString.QValue = newValue.Value;
            return Current.Api.AcqG2Q(newValue);
        }

        public Task<int> Session(int? newValue = null)
        {
            if (newValue != null) connectionString.Session = newValue.Value;
            return Current.Api.AcqG2Session(newValue);
        }

        public Task<int> RFPower(int? newValue = null)
        {
            if (newValue != null) connectionString.RFPower = newValue.Value;
            return Current.Api.RFLevel(newValue);
        }

        public async Task<AntennaConfiguration> AntennaConfiguration(AntennaConfiguration? newValue = null)
        {
            if (newValue != null) connectionString.AntennaConfiguration = newValue.Value;
            return (await Current.Api.AntennaSequence(newValue.ToAlienAntennaSequence())).ParseAlienAntennaSequence();
        }

        private AlienReaderProtocol proto = null;
        public AlienReaderProtocol Current => proto;
        public int ReconnectTimeout { get; set; } = 2000;
        readonly SerialDisposable reconnectDisposable = new();

        private string login = "alien";
        private string password = "password";
        public bool IsConnected => proto?.IsConnected == true;

        // ReSharper disable once UnusedMember.Global
        public ReconnectingAlienReaderProtocol(ConnectionString cs)
        {
            connectionString = cs.Clone();
            endpoint = connectionString.Network;
            keepAliveTimeout = AlienReaderProtocol.DefaultKeepaliveTimeout;
            receiveTimeout = AlienReaderProtocol.DefaultReceiveTimeout;
            usePolling = true;
            login = connectionString.Login;
            password = connectionString.Password;
            onConnected = async api =>
            {
                //await api.AcqG2AntennaCombine(true);
                await api.AcqTime(connectionString.InventoryDuration);
                await api.AcqG2Q(connectionString.QValue);
                await api.AcqG2Session(connectionString.Session);
                await api.RFModulation(RFModulation.DRM);
                await api.RFLevel(connectionString.RFPower);
                await api.AntennaSequence(connectionString.AntennaConfiguration.ToAlienAntennaSequence());
            };
        }

        public ReconnectingAlienReaderProtocol(DnsEndPoint endpoint, Func<IAlienReaderApi, Task> onConnected,
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
                proto.DisposeSafe();
                var arp = new AlienReaderProtocol(keepAliveTimeout, receiveTimeout);
                await arp.ConnectAndLogin(endpoint.Host, endpoint.Port, login, password);
                if (usePolling)
                    await arp.StartTagPolling(tags, errors, heartbeat);
                else
                    await arp.StartTagStreamOld(tags);
                arp.Disconnected += (s, e) => ScheduleReconnect(true);
                if (!arp.IsConnected) 
                    ScheduleReconnect();
                else
                {
                    proto = arp;
                    Logger.Swallow(() => connected.OnNext(true));
                }
            }
            catch (Exception ex)
            {
                Logger.Warning("Could not connect to {endpoint} {ex}", endpoint, ex);
                ScheduleReconnect();
                errors.OnNext(ex);
                Logger.Swallow(() => connected.OnNext(false));
            }

            try
            {
                if (proto.IsConnected)
                    await onConnected(proto.Api);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "OnConnected handler failed {ex}");
                errors.OnNext(ex);
                Logger.Swallow(() => connected.OnNext(false));
            }
        }

        private void ScheduleReconnect(bool report = false)
        {
            if (report)
                Logger.Swallow(() => connected.OnNext(false));
            reconnectDisposable.Disposable = Observable.Timer(TimeSpan.FromMilliseconds(ReconnectTimeout))
                .Subscribe(x =>
                {
                    var t = Connect();
                });
        }

        public void Dispose()
        {
            reconnectDisposable.DisposeSafe();
            Logger.Swallow((Action)tags.OnCompleted);
            tags.DisposeSafe();
            proto.DisposeSafe();
        }
    }
}