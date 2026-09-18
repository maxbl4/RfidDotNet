using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.SemaphoreExt;
using maxbl4.Infrastructure.Extensions.SocketExt;
using maxbl4.RfidDotNet.Exceptions;
using Serilog;

namespace maxbl4.RfidDotNet.AlienTech.Net
{
    public abstract class DuplexProtocol : IDisposable
    {
        static readonly ILogger Logger = Log.ForContext<DuplexProtocol>();
        private readonly int receiveTimeout;
        private readonly SemaphoreSlim sendReceiveSemaphore = new(1);
        private ByteStream stream;
        public ByteStream Stream => stream;
        public bool IsConnected => stream?.IsConnected == true;
        public abstract string IncomingMessageTerminators { get; }
        private bool hasTriedToConnect = false;
        private bool disposed = false;
        
        public event EventHandler Disconnected = (s, e) => { };

        public const int DefaultConnectTimeout = 5000;
        
        protected DuplexProtocol(int receiveTimeout)
        {
            this.receiveTimeout = receiveTimeout;
        }

        protected void Connect(Socket client)
        {
            ThrowIfNotReady();
            Logger.Information("Connect with socket");
            stream = new ByteStream(client, receiveTimeout);
        }

        private void ThrowIfNotReady()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (hasTriedToConnect)
                throw new AlreadyConnectedtException();
            hasTriedToConnect = true;
        }

        protected virtual Task Connect(string host, int port, int timeout = DefaultConnectTimeout)
        {
            Logger.Information("Connect with {host}:{port} timeout {timeout}", host, port, timeout);
            ThrowIfNotReady();
            return Task.Run(() =>
            {
                Logger.Debug("Connect task started");
                using (sendReceiveSemaphore.UseOnce())
                {
                    Logger.Debug("Connect task acquired semaphore");
                    var client = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    using (TimeoutAction.Set(timeout, () => { client.CloseForce(); Logger.Debug("Timeout expired");}))
                        client.ConnectAsync(host, port).Wait(timeout + 100);
                    Logger.Debug("Connect task socket connected");
                    stream = new ByteStream(client, receiveTimeout);
                    Logger.Debug("Connect task completed");
                }
            });
        }

        protected Task<List<string>> SendReceiveRaw(string data, string terminatorsOverride = null)
        {
            return Task.Run(() =>
            {
                ThrowIfDisposed();
                using (sendReceiveSemaphore.UseOnce())
                {
                    ThrowIfDisposed();
                    stream.Send(data);
                    return ReceiveImpl(terminatorsOverride);
                }
            });
        }

        protected Task<List<string>> Receive(string terminatorsOverride = null)
        {
            return Task.Run(() =>
            {
                ThrowIfDisposed();
                using (sendReceiveSemaphore.UseOnce())
                {
                    ThrowIfDisposed();
                    return ReceiveImpl(terminatorsOverride);
                }
            });
        }

        protected Task SendRaw(string data)
        {
            return Task.Run(() =>
            {
                ThrowIfDisposed();
                using (sendReceiveSemaphore.UseOnce())
                {
                    ThrowIfDisposed();
                    stream.Send(data);
                }
            });
        }

        /// <summary>
        /// A request that races with Dispose is normal: the tag poller and the keepalive
        /// timer are still in flight when the connection goes away. Report it as the
        /// domain error everyone already handles, not as ObjectDisposedException or a
        /// NullReferenceException on a stream that is gone.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (disposed || stream == null)
                throw new ConnectionLostException("Connection is closed");
        }

        private List<string> ReceiveImpl(string terminatorsOverride = null)
        {
            while (true)
            {
                var msgs = stream.Read(terminatorsOverride ?? IncomingMessageTerminators);
                OnReceiveAny(msgs);
                if (msgs.Count > 0)
                    return msgs;
            }
        }

        protected virtual void OnReceiveAny(List<string> msgs)
        {
        }

        public virtual void Dispose()
        {
            if (disposed) return;
            disposed = true;
            Logger.Information("Disposing");
            // sendReceiveSemaphore is deliberately NOT disposed. Requests already in
            // flight would hit a disposed semaphore and throw ObjectDisposedException,
            // which is indistinguishable from a real failure in the log — and with a
            // gateway that auto restarts on errors, that noise is expensive.
            // SemaphoreSlim only needs disposal when AvailableWaitHandle was used,
            // and it never is here, so skipping it leaks nothing.
            stream.DisposeSafe();
            stream = null;
            Disconnected(this, EventArgs.Empty);
            Disconnected = null;
        }
    }
}