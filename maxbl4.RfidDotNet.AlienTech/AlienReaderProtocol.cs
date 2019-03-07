using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Enums;
using maxbl4.RfidDotNet.AlienTech.Interfaces;
using maxbl4.RfidDotNet.AlienTech.Net;
using maxbl4.RfidDotNet.AlienTech.TagStream;
using maxbl4.RfidDotNet.Exceptions;
using Serilog;

namespace maxbl4.RfidDotNet.AlienTech
{
    public class AlienReaderProtocol : DuplexProtocol
    {
        static readonly ILogger Logger = Log.ForContext<AlienReaderProtocol>();
        private readonly int keepAliveTimeout;
        private readonly SerialDisposable pollerDisposable = new SerialDisposable();
        public const int DefaultKeepaliveTimeout = 1000;
        public const int DefaultReceiveTimeout = 3000;
        
        public override string IncomingMessageTerminators => "\0";

        private readonly AlienReaderApiImpl api;
        private string host;
        public IAlienReaderApi Api => api;
        public DateTimeOffset LastKeepalive { get; private set; }

        private AlienTagStreamListener tagStreamListener;
        private TagPoller tagPoller;
        private IDisposable timerHandle;
        public AlienTagStreamListener TagStreamListenerOld => tagStreamListener;
        public TagPoller TagPoller => tagPoller;
        
        public AlienReaderProtocol(int keepAliveTimeout = DefaultKeepaliveTimeout, int receiveTimeout = DefaultReceiveTimeout) : base(receiveTimeout)
        {
            if (keepAliveTimeout < 500 || keepAliveTimeout > 60000)
                throw new ArgumentOutOfRangeException(nameof(keepAliveTimeout), "Value should be in range 500-60000 ms");
            if (keepAliveTimeout > receiveTimeout)
                throw new ArgumentException($"{nameof(keepAliveTimeout)} should be less than {nameof(receiveTimeout)}");
            this.keepAliveTimeout = keepAliveTimeout;
            api = new AlienReaderApiImpl(SendReceive);
            if (keepAliveTimeout > 0)
                SetKeepaliveTimer();
        }
        
        protected override async Task Connect(string host, int port, int timeout = DefaultConnectTimeout)
        {
            this.host = host;
            await base.Connect(host, port, timeout);
            var msgs = await Receive(">");
            Logger.Information("Connect receive welcome");
            if (msgs.Count != 1 || !msgs[0].EndsWith("Username"))
                throw new UnexpectedWelcomeMessageException(msgs);
        }

        private async Task Login(string login, string password)
        {
            string response;
            if ((response = await SendReceive(login)) != "")
                throw new LoginFailedException(response);
            if ((response = await SendReceive(password)) != "")
                throw new LoginFailedException(response);
        }
        
        public async Task ConnectAndLogin(Socket socket, string login, string password)
        {
            Logger.Information("ConnectAndLogin<socket>");
            Connect(socket);
            Logger.Debug("ConnectAndLogin<socket> connected");
            await Login(login, password);
            Logger.Debug("ConnectAndLogin<socket> logged in");
        }

        public async Task ConnectAndLogin(string host, int port, string login, string password, int connectTimeout = DefaultConnectTimeout)
        {
            Logger.Information("ConnectAndLogin");
            await Connect(host, port, connectTimeout);
            Logger.Debug("ConnectAndLogin connected");
            await Login(login, password);
            Logger.Debug("ConnectAndLogin logged in");
        }

        public async Task StartTagStreamOld(IObserver<Tag> tags)
        {
            tagStreamListener?.Dispose();
            await api.TagStreamKeepAliveTime(1800);
            await api.TagStreamFormat(ListFormat.Custom);
            await api.TagStreamCustomFormat(TagParser.CustomFormat);
            await api.AutoModeReset();
            await api.Clear();                        
            await api.StreamHeader(true);
            await api.NotifyMode(false);
            var ep = new IPEndPoint(EndpointLookup.GetIpOnTheSameNet(IPAddress.Parse(host)), 0);
            tagStreamListener = new AlienTagStreamListener(ep, tags);
            await api.TagStreamAddress(tagStreamListener.EndPoint);
            await api.TagStreamMode(true);
            await api.AutoMode(true);
        }

        public async Task StartTagPolling(IObserver<Tag> tags, IObserver<Exception> errors)
        {
            await api.TagListFormat(ListFormat.Custom);
            await api.TagListCustomFormat(TagParser.CustomFormat);
            await api.TagStreamFormat(ListFormat.Custom);
            await api.TagStreamCustomFormat(TagParser.CustomFormat);
            await api.AutoModeReset();
            await api.Clear();
            await api.NotifyMode(false);
            await api.AutoMode(true);
            pollerDisposable.Disposable = tagPoller = new TagPoller(api, tags, errors);
        }

        public async Task<string> SendReceive(string data)
        {
            var t = string.Join(IncomingMessageTerminators, await SendReceiveRaw("\x1" + data + "\r\n"))
                .TrimEnd('\r', '\n');
            return t;
        }

        protected override void OnReceiveAny(List<string> msgs)
        {
            LastKeepalive = DateTimeOffset.UtcNow;
            SetKeepaliveTimer();
        }

        void SetKeepaliveTimer()
        {
            timerHandle?.Dispose();
            timerHandle = Observable.Timer(DateTimeOffset.Now.AddMilliseconds(keepAliveTimeout))
                .Subscribe(CheckKeepAlive);
        }

        void CheckKeepAlive(long x)
        {
            try
            {
                SendReceive("").Wait();
                Logger.Information("Keepalive success");
                Observable.Timer(DateTimeOffset.Now.AddMilliseconds(keepAliveTimeout))
                    .Subscribe(CheckKeepAlive);
            }
            catch
            {
                Dispose();
            }
        }

        public override void Dispose()
        {
            Logger.Information("Disposing");
            pollerDisposable.Dispose();
            tagStreamListener?.Dispose();
            base.Dispose();
        }
    }
}