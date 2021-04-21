using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.SemaphoreExt;
using maxbl4.Infrastructure.Extensions.SocketExt;
using maxbl4.RfidDotNet.AlienTech.Buffers;
using maxbl4.RfidDotNet.Exceptions;
using Serilog;

namespace maxbl4.RfidDotNet.AlienTech.Net
{
    public class ByteStream : IDisposable
    {
        static readonly ILogger Logger = Log.ForContext<ByteStream>();
        private readonly Socket socket;
        private readonly int timeout;
        private readonly MessageParser parser = new();
        readonly SemaphoreSlim semaphore = new(1);
        public bool IsConnected => socket.Connected;
        
        public ByteStream(Socket socket, int timeout = 2000)
        {
            this.socket = socket ?? throw new ArgumentNullException(nameof(socket));
            this.timeout = timeout;
            if (!socket.Connected)
                throw new ArgumentException("Socket should be connected", nameof(socket));
            socket.Blocking = true;
            socket.NoDelay = false;
            Logger.Information("Constructed");
        }

        public void Send(string data)
        {
            try
            {
                Logger.Debug("Send {data}", data);
                semaphore.Wait();
                Logger.Debug("Send acquired semaphore");
                using (TimeoutAction.Set(timeout, () => { Close(); Logger.Debug("Send timeout expired"); }))
                    socket.Send(Encoding.ASCII.GetBytes(data));
                Logger.Debug("Send completed");
            }
            catch (Exception ex)
            {
                Logger.Warning("{ex}", ex);
                Close();
                throw new ConnectionLostException("Could not send", ex);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public List<string> Read(string terminators = "\r\n\0")
        {
            Logger.Debug("Read start");
            using (semaphore.UseOnce())
            {
                Logger.Debug("Read acquired semaphore");
                int read;
                try
                {
                    using (TimeoutAction.Set(timeout, () => { Close(); Logger.Debug("Read timeout expired"); }))
                        read = socket.Receive(parser.Buffer, parser.Offset, parser.BufferLength, SocketFlags.None);
                    Logger.Debug("Read completed");
                }
                catch (Exception ex)
                {
                    Logger.Warning("{ex}", ex);
                    Close();
                    throw new ConnectionLostException("Socket error", ex);
                }

                if (read == 0)
                {
                    Logger.Warning("Read recv returned zero bytes");
                    Close();
                    throw new ConnectionLostException("Recv returned zero bytes");
                }

                return parser.Parse(read, terminators).ToList();
            }
        }

        void Close()
        {
            socket.CloseForce();
        }
        
        public void Dispose()
        {
            Logger.Information("Disposing");
            Close();
            semaphore.DisposeSafe();
        }
    }
}