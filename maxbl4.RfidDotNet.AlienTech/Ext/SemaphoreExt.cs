using System;
using System.Reactive.Disposables;
using System.Threading;

namespace maxbl4.RfidDotNet.AlienTech.Ext
{
    public static class SemaphoreExt
    {
        public static IDisposable UseOnce(this SemaphoreSlim semaphore)
        {
            semaphore.Wait();
            return Disposable.Create(() => semaphore.Release());
        }
    }
}