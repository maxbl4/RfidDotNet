using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.SemaphoreExt;
using maxbl4.RfidDotNet.Exceptions;
using maxbl4.RfidDotNet.GenericSerial.Buffers;
using maxbl4.RfidDotNet.GenericSerial.DataAdapters;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using maxbl4.RfidDotNet.GenericSerial.Model;
using maxbl4.RfidDotNet.GenericSerial.Packets;
using RJCP.IO.Ports;

namespace maxbl4.RfidDotNet.GenericSerial
{
    public class SerialReader : IDisposable
    {
        private readonly IDataStreamFactory streamFactory;        
        private readonly SemaphoreSlim sendReceiveSemaphore = new(1);
        
        private readonly Subject<Tag> tags = new();
        public IObservable<Tag> Tags => tags;
        private readonly Subject<Exception> errors = new();
        public IObservable<Exception> Errors => errors;

        public bool ThrowOnIllegalCommandError { get; set; } = true;

        private RealtimeInventoryListener realtimeInventoryListener;
        
        public SerialReader(string serialPortName, int portSpeed = SerialPortFactory.DefaultBaudRate, 
            int dataBits = SerialPortFactory.DefaultDataBits, Parity parity = SerialPortFactory.DefaultParity, 
            StopBits stopBits = SerialPortFactory.DefaultStopBits)
            : this(new SerialPortFactory(serialPortName, portSpeed, dataBits, parity, stopBits)) {}

        public SerialReader(DnsEndPoint endPoint, int networkTimeout = NetworkStreamFactory.DefaultTimeout)
            : this(new NetworkStreamFactory(endPoint)) {}
        
        public SerialReader(IDataStreamFactory streamFactory)
        {
            this.streamFactory = streamFactory;
        }

        public async Task<IEnumerable<ResponseDataPacket>> SendReceive(CommandDataPacket command)
        {
            using (sendReceiveSemaphore.UseOnce())
            {
                try
                {
                    var port = streamFactory.DataStream;
                    var buffer = command.Serialize();
                    var sw = Stopwatch.StartNew();
                    await port.WriteAsync(buffer, 0, buffer.Length);
                    var responsePackets = new List<ResponseDataPacket>();
                    ResponseDataPacket lastResponse;
                    do
                    {
                        var packet = await MessageParser.ReadPacket(port, sw);
                        sw = null;
                        //TODO: Receive failures should be handled by reopening port.
                        if (!packet.Success)
                            throw new ReceiveFailedException(
                                $"Failed to read response from {streamFactory.Description} {packet.ResultType}");
                        responsePackets.Add(lastResponse = new ResponseDataPacket(command.Command, packet.Data, elapsed: packet.Elapsed,
                            errorsObserver: ThrowOnIllegalCommandError ? null : errors));
                    } while (MessageParser.ShouldReadMore(lastResponse));

                    return responsePackets;
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }
        }

        public async Task<uint> GetSerialNumber()
        {
            var responses = await SendReceive(new CommandDataPacket(ReaderCommand.GetReaderSerialNumber));
            return responses.First().GetReaderSerialNumber();
        }

        public async Task<int> GetReaderTemperature()
        {
            var responses = await SendReceive(new CommandDataPacket(ReaderCommand.GetReaderTemperature));
            return responses.First().GetReaderTemperature();
        }
        
        public async Task<Model.ReaderInfo> GetReaderInfo()
        {
            var responses = await SendReceive(new CommandDataPacket(ReaderCommand.GetReaderInformation));
            return responses.First().GetReaderInfo();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval">Value is rounded to 100 ms, should be in 0 - 25500ms</param>
        /// <returns></returns>
        public async Task SetInventoryScanInterval(TimeSpan interval)
        {
            var responses = await SendReceive(CommandDataPacket.SetInventoryScanInterval(interval));
            responses.First().CheckSuccess();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rfPower">Radio transmitter power in db. The range is determined by the reader and should be check in the specs.
        /// The reader my return error</param>
        /// <returns></returns>
        public async Task SetRFPower(byte rfPower)
        {
            var responses = await SendReceive(CommandDataPacket.SetRFPower(rfPower));
            responses.First().CheckSuccess();
        }
        
        public async Task<bool> GetDrmEnabled()
        {
            var responses = await SendReceive(new CommandDataPacket(ReaderCommand.ModifyOrloadDrmConfiguration, (byte)DrmMode.Read));
            return responses.First().GetDrmEnabled() == DrmMode.On;
        }
        
        public async Task<DrmMode> SetDrmEnabled(bool enabled)
        {
            var mode = enabled ? DrmMode.On : DrmMode.Off;
            mode |= DrmMode.Write;
            var responses = await SendReceive(new CommandDataPacket(ReaderCommand.ModifyOrloadDrmConfiguration, 
                (byte)mode));
            return responses.First().GetDrmEnabled();
        }

        /// <summary>
        /// Change reader serial baud rate. After calling this method, you should reconnect using new baud
        /// </summary>
        /// <param name="baud"></param>
        /// <param name="forceOverNetwork"></param>
        /// <returns></returns>
        public async Task SetSerialBaudRate(BaudRates baud, bool forceOverNetwork = false)
        {
            if (!forceOverNetwork && streamFactory is NetworkStreamFactory)
                throw new InvalidOperationException($"You should not change baud rate of the reader connected over the network. " +
                                                    $"While this is possible, most probably you will loose connectivity to reader" +
                                                    $"until you make manual changes on the network host");
            var responses = await SendReceive(new CommandDataPacket(ReaderCommand.SetSerialBaudRate, 
                (byte)baud));
            responses.First().CheckSuccess();
            streamFactory.UpdateBaudRate(baud.ToNumber());
        }
        
        public async Task ClearBuffer()
        {
            var responses = await SendReceive(new CommandDataPacket(ReaderCommand.ClearBuffer));
            responses.First().CheckSuccess();
        }
        
        /// <summary>
        /// Try to put reader into Query/Answer mode
        /// </summary>
        public async Task ActivateOnDemandInventoryMode(bool ignoreError = false)
        {
            realtimeInventoryListener.DisposeSafe();
            var responses = await SendReceive(new CommandDataPacket(ReaderCommand.SetWorkingMode, (byte)ReaderWorkingMode.Answer));
            // If reader is in realtime mode, it may send arbitrary number notification packets.
            
            var resp = responses.FirstOrDefault(x => x.Command == ReaderCommand.SetWorkingMode);
            if (!ignoreError)
            {
                if (resp == null)
                    throw new ReceiveFailedException($"Did not receive an answer for SetWorkingMode command");
                resp.CheckSuccess();
            }
        }

        public async Task ActivateRealtimeInventoryMode(bool withGpioTrigger = false)
        {
            realtimeInventoryListener.DisposeSafe();
            var responses = await SendReceive(new CommandDataPacket(ReaderCommand.SetWorkingMode, 
                (byte)(withGpioTrigger ? ReaderWorkingMode.RealtimeGPIOTriggered : ReaderWorkingMode.Realtime)));
            var resp = responses.FirstOrDefault(x => x.Command == ReaderCommand.SetWorkingMode);
            if (resp == null)
                throw new ReceiveFailedException($"Did not receive an answer for SetWorkingMode command");
            resp.CheckSuccess();
            realtimeInventoryListener = new RealtimeInventoryListener(streamFactory, sendReceiveSemaphore, tags, errors);
        }

        public async Task<int> GetNumberOfTagsInBuffer()
        {
            var responses = await SendReceive(CommandDataPacket.GetNumberOfTagsInBuffer());
            return responses.First().GetNumberOfTagsInBuffer();
        }
        
        public async Task<EpcLength> GetEpcLengthForBufferOperations()
        {
            var responses = await SendReceive(CommandDataPacket.GetEpcLengthForBufferOperations());
            return responses.First().GetEpcLength();
        }
        
        public async Task SetEpcLengthForBufferOperations(EpcLength epcLength)
        {
            var responses = await SendReceive(CommandDataPacket.SetEpcLengthForBufferOperations(epcLength));
            responses.First().CheckSuccess();
        }
        
        public async Task SetAntennaConfiguration(GenAntennaConfiguration configuration)
        {
            var responses = await SendReceive(CommandDataPacket.SetAntennaConfiguration(configuration));
            responses.First().CheckSuccess();
        }
        
        public async Task SetAntennaCheck(bool enable)
        {
            var responses = await SendReceive(CommandDataPacket.SetAntennaCheck(enable));
            responses.First().CheckSuccess();
        }

        public async Task<TagInventoryResult> TagInventory(TagInventoryParams args = null)
        {
            if (args == null) args = new TagInventoryParams();
            var responses = await SendReceive(CommandDataPacket.TagInventory(ReaderCommand.TagInventory, args));
            return new TagInventoryResult(responses);
        }

        public async Task<TagInventoryResult> TagInventoryWithMemoryBuffer(TagInventoryWithBufferParams args = null)
        {
            if (args == null) args = new TagInventoryWithBufferParams(new TagInventoryOptionalParams(TimeSpan.FromMilliseconds(300)));
            var responses = await SendReceive(CommandDataPacket.TagInventory(ReaderCommand.TagInventoryWithMemoryBuffer, args));
            return new TagInventoryResult(responses);
        }

        public async Task<TagBufferResult> GetTagsFromBuffer()
        {
            var responses = await SendReceive(new CommandDataPacket(ReaderCommand.GetTagsFromBuffer));
            return new TagBufferResult(responses);
        }
        
        public async Task SetRealTimeInventoryParameters(RealtimeInventoryParams args = null)
        {
            if (args == null) args = new RealtimeInventoryParams();
            var responses = await SendReceive(CommandDataPacket.SetRealTimeInventoryParameters(args));
            responses.First().CheckSuccess();
        }

        public void Dispose()
        {
            realtimeInventoryListener.DisposeSafe();
            streamFactory.DisposeSafe();
        }
    }
}