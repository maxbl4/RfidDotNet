using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Infrastructure;
using maxbl4.RfidDotNet.GenericSerial.Buffers;
using maxbl4.RfidDotNet.GenericSerial.Model;
using maxbl4.RfidDotNet.GenericSerial.Packets;
using RJCP.IO.Ports;
using Xunit;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    [Collection("Hardware")]
    [Trait("Hardware", "True")]
    public class SerialUnifiedTagStreamTests
    {
        private readonly List<Tag> tags = new();
        private readonly List<Exception> errors = new();
        private readonly List<bool> connected = new();
        private readonly List<DateTime> heartbeats = new();
        
        [Fact]
        public async Task Work()
        {
            _ = StartPolling(true);
        }
        
        [Fact]
        public async Task Hang()
        {
            _ = StartPolling(false);
        }
        
        public async Task StartPolling(bool yield)
        {
            if (yield)
                await Task.Yield();
            var stream = new SerialPortStream("COM5", 57600, 8, Parity.None, StopBits.One)
            {
                ReadTimeout = 3000, WriteTimeout = 200
            };
            stream.Open();
            stream.DiscardInBuffer();
            stream.DiscardOutBuffer();
            var command = CommandDataPacket.TagInventory(ReaderCommand.TagInventory, new TagInventoryParams());
            var buffer = command.Serialize();
            while (true)
            {
                await stream.WriteAsync(buffer, 0, buffer.Length);
                var responsePackets = new List<ResponseDataPacket>();
                ResponseDataPacket lastResponse;
                do
                {
                    var packet = await MessageParser.ReadPacket(stream);
                    responsePackets.Add(lastResponse = new ResponseDataPacket(command.Command, packet.Data, elapsed: packet.Elapsed));
                } while (MessageParser.ShouldReadMore(lastResponse));
            }
        }

        private void SubscribeStreams(IUniversalTagStream stream)
        {
            stream.Connected.Subscribe(connected.Add);
            stream.Tags.Subscribe(tags.Add);
            stream.Errors.Subscribe(errors.Add);
            stream.Heartbeat.Subscribe(heartbeats.Add);
        }
    }
}