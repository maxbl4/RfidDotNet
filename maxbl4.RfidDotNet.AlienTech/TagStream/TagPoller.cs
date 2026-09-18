using System;
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Interfaces;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using Serilog;

namespace maxbl4.RfidDotNet.AlienTech.TagStream
{
    public class TagPoller : IDisposable
    {
        static readonly ILogger Logger = Log.ForContext<TagPoller>();
        const int ErrorBackoffMs = 200;
        const int IdleSleepMs = 1;
        private readonly AlienReaderApi api;
        private readonly IObserver<Tag> tags;
        private readonly IObserver<Exception> errors;
        private readonly IObserver<DateTime> heartbeat;
        private volatile bool run = true;
        readonly Subject<string> unparsedMessages = new();
        readonly ConcurrentQueue<Tag> inventoryResults = new();
        public IObservable<string> UnparsedMessages => unparsedMessages;

        public TagPoller(AlienReaderApi api, IObserver<Tag> tags, IObserver<Exception> errors, IObserver<DateTime> heartbeat)
        {
            this.api = api;
            this.tags = tags;
            this.errors = errors;
            this.heartbeat = heartbeat;
            Logger.Information("Starting");
            new Task(PollingThread, TaskCreationOptions.LongRunning).Start();
            new Task(StreamingThread, TaskCreationOptions.LongRunning).Start();
        }

        async void PollingThread()
        {
            var lastHeartbeat = DateTime.UtcNow;
            while (run)
            {
                try
                {
                    var s = await api.TagList();
                    var lines = s.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (line == ProtocolMessages.NoTags)
                            continue;
                        if (TagParser.TryParse(line, out var t))
                        {
                            t.DiscoveryTime = t.LastSeenTime = DateTime.UtcNow;
                            inventoryResults.Enqueue(t);
                        }
                        else
                            unparsedMessages.OnNext(line);
                    }
                    if (DateTime.UtcNow - lastHeartbeat > TimeSpan.FromSeconds(1))
                    {
                        inventoryResults.Enqueue(null);
                        lastHeartbeat = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    // A failure right after Dispose is expected: the connection is gone
                    // and that is exactly why we are stopping. Reporting it would make
                    // a normal shutdown look like a reader fault.
                    if (!run) return;
                    Logger.Warning(ex, "TagPoller failed to poll the reader");
                    errors.OnNext(ex);
                    // Without this the loop spins on a dead connection, filling the log
                    // with the same exception thousands of times per second.
                    await Task.Delay(ErrorBackoffMs);
                }
            }
        }

        void StreamingThread()
        {
            while (run)
            {
                try
                {
                    if (inventoryResults.TryDequeue(out var tag))
                    {
                        if (!run) return;
                        if (tag != null)
                        {
                            tags.OnNext(tag);
                        }
                        else
                        {
                            heartbeat.OnNext(DateTime.UtcNow);
                        }
                        continue;
                    }
                    // Thread.Yield() here burned a whole core on an idle reader. On the
                    // four weak cores of an OrangePi that is a quarter of the machine
                    // spent on nothing; a millisecond of sleep is invisible against the
                    // 400 ms budget of "проезд → рейтинг".
                    Thread.Sleep(IdleSleepMs);
                }
                catch (Exception e)
                {
                    if (!run) return;
                    errors.OnNext(e);
                }
            }
        }

        public void Dispose()
        {
            run = false;
        }
    }
}