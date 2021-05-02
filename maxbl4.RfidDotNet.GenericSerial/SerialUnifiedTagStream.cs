using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.RfidDotNet.Exceptions;
using maxbl4.RfidDotNet.GenericSerial.Buffers;
using maxbl4.RfidDotNet.GenericSerial.Exceptions;
using maxbl4.RfidDotNet.GenericSerial.Model;
using maxbl4.RfidDotNet.GenericSerial.Packets;
using RJCP.IO.Ports;
using Serilog;

namespace maxbl4.RfidDotNet.GenericSerial
{
    public class SerialUnifiedTagStream : IUniversalTagStream
    {
        static readonly ILogger Logger = Log.ForContext<SerialUnifiedTagStream>();
        private readonly ConnectionString connectionString;
        private readonly SerialReader serialReaderSafe;

        private readonly Channel<TagInventoryResult> inventoryResults =
            Channel.CreateBounded<TagInventoryResult>(1000000);

        public const int DefaultTemperatureLimitCheckInterval = 30000;
        public int TemperatureLimitCheckInterval { get; set; } = DefaultTemperatureLimitCheckInterval;

        public SerialUnifiedTagStream(ConnectionString cs)
        {
            connectionString = cs.Clone();
            //serialReaderSafe = new SerialReader(connectionString, connected, errors);
            serialReaderSafe = new SerialReader(new SerialConnectionString(connectionString).Connect())
            {
                ThrowOnIllegalCommandError = false
            };
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
            return default;
        }
        public bool Start2()
        {
            _ = StartPolling();
            return true;
        }

        public async Task StartPolling()
        {
            //await Task.Yield();
            var stream = new SerialPortStream("COM5", 57600, 8, Parity.None, StopBits.One)
            {
                ReadTimeout = 3000, WriteTimeout = 200
            };
            stream.Open();
            stream.DiscardInBuffer();
            stream.DiscardOutBuffer();
            var command = CommandDataPacket.TagInventory(ReaderCommand.TagInventory, new TagInventoryParams
            {
                QValue = (byte)connectionString.QValue,
                Session = (SessionValue)connectionString.Session
            });
            while (doInventory)
            {
                var buffer = command.Serialize();
                var sw = Stopwatch.StartNew();
                await stream.WriteAsync(buffer, 0, buffer.Length);
                var responsePackets = new List<ResponseDataPacket>();
                ResponseDataPacket lastResponse;
                do
                {
                    var packet = await MessageParser.ReadPacket(stream, sw);
                    sw = null;
                    responsePackets.Add(lastResponse = new ResponseDataPacket(command.Command, packet.Data, elapsed: packet.Elapsed));
                } while (MessageParser.ShouldReadMore(lastResponse));
                var res = new TagInventoryResult(responsePackets);
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
            return default;
        }

        public async Task<AntennaConfiguration> AntennaConfiguration(AntennaConfiguration? newValue = null)
        {
            return default;
        }
    }
}