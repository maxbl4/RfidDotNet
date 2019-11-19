using System;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.SemaphoreExt;
using maxbl4.RfidDotNet.GenericSerial.DataAdapters;
using maxbl4.RfidDotNet.GenericSerial.Packets;

namespace maxbl4.RfidDotNet.GenericSerial.Buffers
{
    public class RealtimeInventoryListener : IDisposable
    {
        private readonly IDataStreamFactory dataStreamFactory;
        private readonly SemaphoreSlim semaphore;
        private readonly IObserver<Tag> tags;
        private readonly IObserver<Exception> errors;
        private Task loop;
        private bool run = true;

        public RealtimeInventoryListener(IDataStreamFactory dataStreamFactory, SemaphoreSlim semaphore, IObserver<Tag> tags, IObserver<Exception> errors)
        {
            this.dataStreamFactory = dataStreamFactory;
            this.semaphore = semaphore;
            this.tags = tags;
            this.errors = errors;
            loop = ListenLoop();
        }

        async Task ListenLoop()
        {
            using (semaphore.UseOnce())
            {
                try
                {
                    while (run)
                    {
                        var packet = await MessageParser.ReadPacket(dataStreamFactory.DataStream);
                        if (packet.Success)
                        {
                            var msg = new ResponseDataPacket(ReaderCommand.RealtimeInventoryResponse, packet.Data,
                                elapsed: packet.Elapsed);
                        
                            var t = msg.GetRealtimeTag(out var isHeartbeat);
                            if (t != null)
                                tags.OnNext(t);
                        }else
                            Console.WriteLine(packet.ResultType);
                    }
                }
                catch (Exception ex)
                {
                    errors.OnNext(ex);
                }
            }
        }


        public void Dispose()
        {
            run = false;
        }
    }
}