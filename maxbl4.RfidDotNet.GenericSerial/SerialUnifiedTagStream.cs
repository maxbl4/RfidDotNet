using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.RfidDotNet.Exceptions;
using maxbl4.RfidDotNet.GenericSerial.Exceptions;
using maxbl4.RfidDotNet.GenericSerial.Model;
using Serilog;

namespace maxbl4.RfidDotNet.GenericSerial
{
    public class SerialUnifiedTagStream : IUniversalTagStream
    {
        static readonly ILogger Logger = Log.ForContext<SerialUnifiedTagStream>();
        private readonly ConnectionString connectionString;
        private readonly SerialReaderSafe serialReaderSafe;

        private readonly Channel<TagInventoryResult> inventoryResults =
            Channel.CreateBounded<TagInventoryResult>(1000000);

        public const int DefaultTemperatureLimitCheckInterval = 30000;
        public int TemperatureLimitCheckInterval { get; set; } = DefaultTemperatureLimitCheckInterval;

        public SerialUnifiedTagStream(ConnectionString cs)
        {
            connectionString = cs.Clone();
            serialReaderSafe = new SerialReaderSafe(connectionString, connected, errors);
        }

        public void Dispose()
        {
            doInventory = false;
            serialReaderSafe.DisposeSafe();
        }

        readonly Subject<Tag> tags = new();
        public IObservable<Tag> Tags => tags;
        readonly Subject<Exception> errors = new();
        public IObservable<Exception> Errors => errors;
        readonly BehaviorSubject<bool> connected = new(false);
        public IObservable<DateTime> Heartbeat => heartbeat;
        readonly BehaviorSubject<DateTime> heartbeat = new(DateTime.MinValue);
        private bool doInventory = true;
        public IObservable<bool> Connected => connected;
        public Task Start()
        {
            _ = StartPolling();
            _ = StartStreamingTags();
            connected.OnNext(true);
            return Task.CompletedTask;
        }

        private async Task StartPolling()
        {
            await Task.Yield();
            var sw = new Stopwatch();
            if (connectionString.ThermalLimit > 0)
                sw.Start();
            var lastHeartbeat = DateTime.UtcNow;
            while (doInventory)
            {
                try
                {
                    var res = await serialReaderSafe.Do(x => x.TagInventory(new TagInventoryParams
                    {
                        QValue = (byte)connectionString.QValue,
                        Session = (SessionValue)connectionString.Session
                    }));
                    
                    if (!inventoryResults.Writer.TryWrite(res))
                        errors.OnNext(new TagReadBufferIsFull());
                    if (sw.ElapsedMilliseconds > TemperatureLimitCheckInterval)
                    {
                        var t = await serialReaderSafe.Do(x => x.GetReaderTemperature());
                        if (t > connectionString.ThermalLimit)
                        {
                            errors.OnNext(new TemperatureLimitExceededException(connectionString.ThermalLimit, t));
                            await Task.Delay(10000);
                        }
                        sw.Restart();
                    }
                    if (DateTime.UtcNow - lastHeartbeat > TimeSpan.FromSeconds(1))
                    {
                        Logger.Swallow(() => heartbeat.OnNext(DateTime.UtcNow));
                        lastHeartbeat = DateTime.UtcNow;
                    }
                }
                catch (Exception e)
                {
                    errors.OnNext(e);
                }
            }
        }
        
        private async Task StartStreamingTags()
        {
            while (doInventory)
            {
                try
                {
                    var res = await inventoryResults.Reader.ReadAsync();
                    foreach (var tag in res.Tags)
                    {
                        tags.OnNext(tag);
                    }
                }
                catch (Exception e)
                {
                    errors.OnNext(e);
                }
            }
        }

        public Task<int> QValue(int? newValue = null)
        {
            if (newValue != null) connectionString.QValue = newValue.Value;
            return Task.FromResult(connectionString.QValue);
        }

        public Task<int> Session(int? newValue = null)
        {
            if (newValue != null) connectionString.Session = newValue.Value;
            return Task.FromResult(connectionString.Session);
        }

        public async Task<int> RFPower(int? newValue = null)
        {
            if (newValue != null)
            {
                connectionString.RFPower = newValue.Value;
                serialReaderSafe.UpdateConnectionString(connectionString);
            }

            var info = await serialReaderSafe.Do(x => x.GetReaderInfo());
            return info?.RFPower ?? 0;
        }

        public async Task<AntennaConfiguration> AntennaConfiguration(AntennaConfiguration? newValue = null)
        {
            if (newValue != null)
            {
                connectionString.AntennaConfiguration = newValue.Value;
                serialReaderSafe.UpdateConnectionString(connectionString);
            }
            var info = await serialReaderSafe.Do(x => x.GetReaderInfo());
            return (AntennaConfiguration?)info?.AntennaConfiguration ?? RfidDotNet.AntennaConfiguration.Nothing;
        }
    }
}