using System;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.GenericSerial.Buffers;
using maxbl4.RfidDotNet.GenericSerial.DataAdapters;
using Serilog;

namespace maxbl4.RfidDotNet.GenericSerial.Sl130
{
    public sealed class Sl130UnifiedTagStream : IUniversalTagStream
    {
        private static readonly ILogger Logger = Log.ForContext<Sl130UnifiedTagStream>();
        private readonly ConnectionString connectionString;
        private readonly IDataStreamFactory streamFactory;
        private readonly Subject<Tag> tags = new();
        private readonly Subject<Exception> errors = new();
        private readonly BehaviorSubject<bool> connected = new(false);
        private readonly BehaviorSubject<DateTime> heartbeat = new(DateTime.MinValue);
        private readonly SemaphoreSlim commandLock = new(1);
        private readonly SemaphoreSlim ioLock = new(1);
        private readonly object responseSync = new();

        private Stream stream;
        private CancellationTokenSource cts;
        private Task readLoop;
        private Task heartbeatLoop;
        private TaskCompletionSource<byte[]> pendingResponse;
        private byte pendingCommand;
        private bool disposed;

        public Sl130UnifiedTagStream(ConnectionString connectionString)
            : this(connectionString, new SerialConnectionString(connectionString).Connect()) {}

        public Sl130UnifiedTagStream(ConnectionString connectionString, IDataStreamFactory streamFactory)
        {
            this.connectionString = connectionString.Clone();
            this.connectionString.AntennaConfiguration = RfidDotNet.AntennaConfiguration.Antenna1;
            this.streamFactory = streamFactory;
        }

        public IObservable<Tag> Tags => tags;
        public IObservable<Exception> Errors => errors;
        public IObservable<bool> Connected => connected;
        public IObservable<DateTime> Heartbeat => heartbeat;

        public Task Start()
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (cts != null) return Task.CompletedTask;

            cts = new CancellationTokenSource();
            try
            {
                stream = streamFactory.DataStream;
                if (stream.CanTimeout)
                {
                    stream.ReadTimeout = 500;
                    stream.WriteTimeout = 500;
                }

                var current = Sl130Protocol.ParseWorkMode(SendDirect(Sl130Protocol.GetWorkMode));
                var required = Sl130Protocol.ForContinuousInventory(current);
                if (current != required)
                {
                    Sl130Protocol.ValidateResponse(
                        SendDirect(Sl130Protocol.SetWorkMode, Sl130Protocol.SerializeWorkMode(required)),
                        Sl130Protocol.SetWorkMode,
                        6);
                }

                var info = Sl130Protocol.ParseReaderInfo(SendDirect(Sl130Protocol.GetReaderInformation));
                if (info.RfPower != connectionString.RFPower)
                {
                    Sl130Protocol.ValidateResponse(
                        SendDirect(Sl130Protocol.SetRfPower, (byte)connectionString.RFPower),
                        Sl130Protocol.SetRfPower,
                        6);
                }

                connected.OnNext(true);
                heartbeat.OnNext(DateTime.UtcNow);
                readLoop = Task.Run(() => ReadLoop(cts.Token));
                heartbeatLoop = Task.Run(() => HeartbeatLoop(cts.Token));
                return Task.CompletedTask;
            }
            catch
            {
                cts.Cancel();
                streamFactory.Dispose();
                cts.Dispose();
                cts = null;
                throw;
            }
        }

        public Task<int> QValue(int? newValue = null)
        {
            if (newValue.HasValue) connectionString.QValue = newValue.Value;
            return Task.FromResult(connectionString.QValue);
        }

        public Task<int> Session(int? newValue = null)
        {
            if (newValue.HasValue) connectionString.Session = newValue.Value;
            return Task.FromResult(connectionString.Session);
        }

        public async Task<int> RFPower(int? newValue = null)
        {
            if (newValue.HasValue)
            {
                var power = (byte)newValue.Value;
                Sl130Protocol.ValidateResponse(
                    await SendCommand(Sl130Protocol.SetRfPower, default, power),
                    Sl130Protocol.SetRfPower,
                    6);
                connectionString.RFPower = power;
            }
            return connectionString.RFPower;
        }

        public Task<AntennaConfiguration> AntennaConfiguration(AntennaConfiguration? newValue = null)
        {
            if (newValue.HasValue && newValue.Value != RfidDotNet.AntennaConfiguration.Antenna1)
                throw new ArgumentException("SL130 has only antenna 1", nameof(newValue));
            connectionString.AntennaConfiguration = RfidDotNet.AntennaConfiguration.Antenna1;
            return Task.FromResult(RfidDotNet.AntennaConfiguration.Antenna1);
        }

        private byte[] SendDirect(byte command, params byte[] data)
        {
            var packet = Sl130Protocol.BuildCommand(command, data);
            stream.Write(packet, 0, packet.Length);
            for (var attempt = 0; attempt < 20; attempt++)
            {
                var result = MessageParser.ReadPacket(stream).GetAwaiter().GetResult();
                if (!result.Success)
                {
                    if (result.ResultType == PacketResultType.Timeout) continue;
                    throw new InvalidDataException($"SL130 response read failed: {result.ResultType}");
                }
                if (result.Data[2] == command) return result.Data;
                PublishTag(result.Data);
            }
            throw new TimeoutException($"SL130 command 0x{command:X2} did not return a response");
        }

        private async Task<byte[]> SendCommand(byte command, CancellationToken ct, params byte[] data)
        {
            await commandLock.WaitAsync(ct);
            var completion = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
            try
            {
                lock (responseSync)
                {
                    if (pendingResponse != null) throw new InvalidOperationException("SL130 command is already pending");
                    pendingCommand = command;
                    pendingResponse = completion;
                }
                var packet = Sl130Protocol.BuildCommand(command, data);
                await ioLock.WaitAsync(ct);
                try
                {
                    await stream.WriteAsync(packet, 0, packet.Length, ct);
                }
                finally
                {
                    ioLock.Release();
                }
                return await completion.Task.WaitAsync(TimeSpan.FromSeconds(2), ct);
            }
            finally
            {
                lock (responseSync)
                {
                    pendingResponse = null;
                    completion.TrySetCanceled();
                }
                commandLock.Release();
            }
        }

        private void ReadLoop(CancellationToken ct)
        {
            var crcErrors = 0;
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    PacketResult result;
                    ioLock.Wait(ct);
                    try
                    {
                        result = MessageParser.ReadPacket(stream).GetAwaiter().GetResult();
                    }
                    catch (TimeoutException)
                    {
                        continue;
                    }
                    finally
                    {
                        ioLock.Release();
                    }
                    if (!result.Success)
                    {
                        if (result.ResultType == PacketResultType.Timeout) continue;
                        if (result.ResultType == PacketResultType.WrongCrc)
                        {
                            crcErrors++;
                            Logger.Warning("SL130 frame CRC mismatch: {Frame}", Convert.ToHexString(result.Data));
                            errors.OnNext(new InvalidDataException("SL130 frame CRC mismatch"));
                            if (crcErrors >= 3) throw new InvalidDataException("SL130 lost frame synchronization");
                            continue;
                        }
                        throw new InvalidDataException("SL130 frame is incomplete");
                    }
                    crcErrors = 0;
                    ProcessFrame(result.Data);
                }
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                connected.OnNext(false);
                errors.OnNext(ex);
                cts.Cancel();
            }
        }

        private async Task HeartbeatLoop(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                    await SendCommand(Sl130Protocol.GetWorkMode, ct);
                    heartbeat.OnNext(DateTime.UtcNow);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) {}
            catch (Exception ex)
            {
                errors.OnNext(ex);
                connected.OnNext(false);
                cts.Cancel();
            }
        }

        private void ProcessFrame(byte[] frame)
        {
            TaskCompletionSource<byte[]> completion = null;
            lock (responseSync)
            {
                if (pendingResponse != null && frame.Length > 2 && frame[2] == pendingCommand)
                {
                    completion = pendingResponse;
                    pendingResponse = null;
                }
            }
            if (completion != null)
            {
                completion.TrySetResult(frame);
                return;
            }
            PublishTag(frame);
        }

        private void PublishTag(byte[] frame)
        {
            if (Sl130Protocol.TryCreateTag(frame, DateTime.UtcNow, out var tag)) tags.OnNext(tag);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            try { cts?.Cancel(); } catch {}
            streamFactory.Dispose();
            lock (responseSync)
            {
                pendingResponse?.TrySetCanceled();
                pendingResponse = null;
            }
            try { readLoop?.Wait(1000); } catch {}
            try { heartbeatLoop?.Wait(1000); } catch {}
            connected.OnNext(false);
            cts?.Dispose();
            commandLock.Dispose();
            ioLock.Dispose();
        }
    }
}
