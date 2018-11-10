using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace maxbl4.RfidDotNet.AlienTech.Tests.Infrastructure
{
    public class Timing
    {
        public static Task<bool> StartWait(Func<bool> predicate, int timeout, int? pollingInterval = null)
        {
            return StartWait(predicate, TimeSpan.FromMilliseconds(timeout),
                pollingInterval.HasValue ? (TimeSpan?)TimeSpan.FromMilliseconds(pollingInterval.Value) : null);
        }

        public static async Task<bool> StartWait(Func<bool> predicate, TimeSpan? timeout = null, TimeSpan? pollingInterval = null)
        {
            if (timeout == null) timeout = TimeSpan.FromMilliseconds(10000);
            if (pollingInterval == null) pollingInterval = TimeSpan.FromMilliseconds(100);
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                if (predicate())
                    return true;
                await Task.Delay(pollingInterval.Value);
            }
            return false;
        }
    }
}