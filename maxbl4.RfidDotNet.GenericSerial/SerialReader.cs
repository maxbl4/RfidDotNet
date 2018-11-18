using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.Exceptions;
using maxbl4.RfidDotNet.Ext;
using maxbl4.RfidDotNet.GenericSerial.Buffers;
using maxbl4.RfidDotNet.GenericSerial.Model;
using maxbl4.RfidDotNet.GenericSerial.Packets;
using RJCP.IO.Ports;

namespace maxbl4.RfidDotNet.GenericSerial
{
    public class SerialReader : IDisposable
    {
        private readonly string serialPortName;
        public const int DefaultPortSpeed = 57600;
        private readonly SemaphoreSlim sendReceiveSemaphore = new SemaphoreSlim(1);
        private readonly SerialPortStream port;

        public SerialReader(string serialPortName)
        {
            this.serialPortName = serialPortName;
            this.port = new SerialPortStream(serialPortName, DefaultPortSpeed, 8, Parity.None, StopBits.One);
            port.ReadTimeout = 3000;
            port.WriteTimeout = 200;
            port.Open();
        }

        public async Task<IEnumerable<ResponseDataPacket>> SendReceive(CommandDataPacket command)
        {
            using (sendReceiveSemaphore.UseOnce())
            {
                port.DiscardInBuffer();
                port.DiscardOutBuffer();
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
                            $"Failed to read response from {serialPortName} {packet.ResultType}");
                    responsePackets.Add(lastResponse = new ResponseDataPacket(command.Command, packet.Data));
                } while (MessageParser.ShouldReadMore(lastResponse));
                
                return responsePackets;
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
            port?.Dispose();
        }
    }
}