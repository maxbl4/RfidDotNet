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
        public async Task Hang()
        {
            var command = CommandDataPacket.TagInventory(ReaderCommand.TagInventory, new TagInventoryParams());
            var buffer = command.Serialize();
            _ = StartPolling(false, buffer);
        }
        
        public async Task StartPolling(bool yield, byte[] buffer)
        {
            // await Task.Yield(); uncomment and it will unblock
            var stream = new SerialPortStream("COM5", 57600, 8, Parity.None, StopBits.One)
            {
                ReadTimeout = 3000, WriteTimeout = 200
            };
            stream.Open();
            stream.DiscardInBuffer();
            stream.DiscardOutBuffer();
            
            while (true)
            {
                await stream.WriteAsync(buffer, 0, buffer.Length);
                var packetLength = stream.ReadByte();
                if (packetLength < 0)
                    throw new Exception();
                
                var totalRead = 0;
                var data = new byte[packetLength + 1];
                data[0] = (byte)packetLength;
                while (totalRead < packetLength)
                {
                    var read = await stream.ReadAsync(data, totalRead + 1, packetLength - totalRead);
                    if (read == 0)
                        throw new Exception();
                    totalRead += read;
                }

                totalRead.Should().Be(packetLength);
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