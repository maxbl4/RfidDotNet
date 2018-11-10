using System;
using System.Reactive.Subjects;
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
        private bool run = true;
        readonly Subject<string> unparsedMessages = new Subject<string>();
        public IObservable<string> UnparsedMessages => unparsedMessages;

        public TagPoller(AlienReaderApi api, IObserver<Tag> tags)
        {
            this.api = api;
            this.tags = tags;
            Logger.Information("Starting");
            new Task(PollingThread, TaskCreationOptions.LongRunning).Start();
        }

        async void PollingThread()
        {
            try
            {
                while (run)
                {
                    var s = await api.TagList();
                    var lines = s.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (line == ProtocolMessages.NoTags)
                            continue;
                        if (TagParser.TryParse(line, out var t))
                            tags.OnNext(t);
                        else
                            unparsedMessages.OnNext(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("{ex}", ex);
            }
        }

        public void Dispose()
        {
            run = false;
        }
    }
}