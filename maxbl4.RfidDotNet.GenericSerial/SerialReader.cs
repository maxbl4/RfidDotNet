using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.Exceptions;
using maxbl4.RfidDotNet.Ext;
using maxbl4.RfidDotNet.GenericSerial.Buffers;
using maxbl4.RfidDotNet.GenericSerial.DataAdapters;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using maxbl4.RfidDotNet.GenericSerial.Model;
using maxbl4.RfidDotNet.GenericSerial.Packets;
using RJCP.IO.Ports;
using ProtocolType = System.Net.Sockets.ProtocolType;

namespace maxbl4.RfidDotNet.GenericSerial
{
    public class SerialReader : IDisposable
    {
        private readonly IDataStreamFactory streamFactory;        
        private readonly SemaphoreSlim sendReceiveSemaphore = new SemaphoreSlim(1);
        
        public SerialReader(string serialPortName, int portSpeed = SerialPortFactory.DefaultPortSpeed, 
            int dataBits = SerialPortFactory.DefaultDataBits, Parity parity = SerialPortFactory.DefaultParity, 
            StopBits stopBits = SerialPortFactory.DefaultStopBits)
            : this(new SerialPortFactory(serialPortName, portSpeed, dataBits, parity, stopBits)) {}

        public SerialReader(IPEndPoint targetEndpoint, int networkTimeout = NetworkStreamFactory.DefaultTimeout)
            : this(new NetworkStreamFactory(targetEndpoint)) {}
        
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
                    await port.WriteAsync(buffer, 0, buffer.Length);
                    var responsePackets = new List<ResponseDataPacket>();
                    ResponseDataPacket lastResponse;
                    do
                    {
                        var packet = await MessageParser.ReadPacket(port);
                        //TODO: Receive failures should be handled by reopening port.
                        if (!packet.Success)
                            throw new ReceiveFailedException(
                                $"Failed to read response from {streamFactory.Description} {packet.ResultType}");
                        responsePackets.Add(lastResponse = new ResponseDataPacket(command.Command, packet.Data));
                    } while (MessageParser.ShouldReadMore(lastResponse));

                    return responsePackets;
                }
                catch (ReceiveFailedException)
                {
                    Dispose();
                    throw;
                }
                catch (NotSupportedException)
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
        
        public async Task ClearBuffer()
        {
            var responses = await SendReceive(new CommandDataPacket(ReaderCommand.ClearBuffer));
            responses.First().CheckSuccess();
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
        
        public async Task SetAntennaConfiguration(AntennaConfiguration configuration)
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

        public void Dispose()
        {
            streamFactory.DisposeSafe();
        }
    }
}