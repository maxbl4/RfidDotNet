using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
    public class SerialUnifiedTagStreamTests
    {
        [Fact]
        public async Task Com0ComRepro()
        {
            var state = new State
            {
                Sw = Stopwatch.StartNew()
            };
            // These methods are expected to not block, because they return Task
            // On version 2.3.1 Client method blocks and executes synchronously
            // It does not hang, the loop inside is working, this is indicated by
            // SuccessCount > 10 (which worked for me)
            // But Sw.ElapsedMilliseconds indicate >5000 because Client() method blocked and test fails
            // If I add Task.Yield() to Client() test passes, also test passes on version 2.2.2 without Yield()
            _ = Device(state);
            _ = Client(state);
            await Task.Delay(500);
            state.SuccessCount.Should().BeGreaterThan(10);
            state.Sw.ElapsedMilliseconds.Should().BeLessThan(1000);
        }
        
        async Task Device(State state)
        {
            var port = new SerialPortStream("COM8", 57600, 8, Parity.None, StopBits.One)
            {
                ReadTimeout = 3000, WriteTimeout = 200
            };
            port.Open();
            port.DiscardInBuffer();
            port.DiscardOutBuffer();
            var buffer = new byte[100];
            while (state.Sw.ElapsedMilliseconds < 5000)
            {
                var read = await port.ReadAsync(buffer);
                if (read > 0)
                {
                    buffer[0] = 1;
                    await port.WriteAsync(buffer, 0, read);
                }
            }
        }
            
        async Task Client(State state)
        {
            //await Task.Yield();
            var port = new SerialPortStream("COM7", 57600, 8, Parity.None, StopBits.One)
            {
                ReadTimeout = 3000, WriteTimeout = 200
            };
            port.Open();
            port.DiscardInBuffer();
            port.DiscardOutBuffer();
            var buffer = new byte[] { 6,0,1,4,0,172,54};
            while (state.Sw.ElapsedMilliseconds < 5000)
            {
                await port.WriteAsync(buffer, 0, buffer.Length);
                var packetLength = port.ReadByte(); // adding this line make method blocking and not return Task
                var read = await port.ReadAsync(buffer);
                if (read > 0)
                    state.SuccessCount++;
            }
        }

        class State
        {
            public int SuccessCount;
            public Stopwatch Sw;
        }
    }
}