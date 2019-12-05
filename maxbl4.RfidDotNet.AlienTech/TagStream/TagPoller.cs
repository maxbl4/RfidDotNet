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
        private readonly AlienReaderApi api;
        private readonly IObserver<Tag> tags;
        private readonly IObserver<Exception> errors;
        private readonly IObserver<DateTime> heartbeat;
        private bool run = true;
        readonly Subject<string> unparsedMessages = new Subject<string>();
        readonly ConcurrentQueue<Tag> inventoryResults = new ConcurrentQueue<Tag>();
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
                    Logger.Warning(ex, "TagPoller stopped");
                    errors.OnNext(ex);
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
                        if (tag != null)
                        {
                            tags.OnNext(tag);
                        }
                        else
                        {
                            heartbeat.OnNext(DateTime.UtcNow);
                        }
                    }

                    Thread.Yield();
                }
                catch (Exception e)
                {
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