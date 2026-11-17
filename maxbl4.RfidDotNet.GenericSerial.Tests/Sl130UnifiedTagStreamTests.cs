using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using maxbl4.RfidDotNet.GenericSerial.DataAdapters;
using maxbl4.RfidDotNet.GenericSerial.Sl130;
using Xunit;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    public class Sl130UnifiedTagStreamTests
    {
        [Fact]
        public async Task Pull_should_not_wait_for_read_timeout_between_inventories()
        {
            var factory = new Sl130StreamFactory();
            var connectionString = ConnectionString.Parse(
                "Protocol=Sl130;Sl130Mode=Pull;Serial=COM1@57600;RFPower=9");
            using var stream = new Sl130UnifiedTagStream(connectionString, factory);
            var tags = new List<Tag>();
            stream.Tags.Subscribe(tags.Add);

            await stream.Start();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            while (factory.InventoryCommands < 5 || tags.Count < 15)
            {
                timeout.Token.ThrowIfCancellationRequested();
                await Task.Delay(10, timeout.Token);
            }

            factory.WorkModeCommands.Should().Be(1);
            factory.InventoryCommands.Should().BeGreaterThanOrEqualTo(5);
            tags.Count.Should().BeGreaterThanOrEqualTo(15);
        }

        private sealed class Sl130StreamFactory : IDataStreamFactory
        {
            private readonly Sl130TestStream stream = new();
            public int InventoryCommands => stream.InventoryCommands;
            public int WorkModeCommands => stream.WorkModeCommands;
            public Stream DataStream => stream;
            public string Description => "SL130 test";
            public void Invalidate() => stream.Dispose();
            public void UpdateBaudRate(int baudRate) {}
            public void Dispose() => stream.Dispose();
        }

        private sealed class Sl130TestStream : Stream
        {
            private readonly BlockingCollection<byte> input = new();
            public int InventoryCommands;
            public int WorkModeCommands;

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override bool CanTimeout => true;
            public override int ReadTimeout { get; set; } = 500;
            public override int WriteTimeout { get; set; } = 500;
            public override long Length => throw new NotSupportedException();
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (!input.TryTake(out var value, ReadTimeout)) throw new TimeoutException();
                buffer[offset] = value;
                return 1;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                var command = buffer[offset + 2];
                if (command == Sl130Protocol.TagInventory) Interlocked.Increment(ref InventoryCommands);
                if (command == Sl130Protocol.GetWorkMode) Interlocked.Increment(ref WorkModeCommands);
                var response = command switch
                {
                    Sl130Protocol.GetWorkMode => WithCrc(new byte[]
                    {
                        0x11, 0x00, 0x36, 0x00, 0x00, 0x01, 0x0A, 0x0F, 0x00,
                        0x08, 0x05, 0x09, 0x01, 0x00, 0x08, 0x05, 0x00, 0x00,
                    }),
                    Sl130Protocol.GetReaderInformation => WithCrc(new byte[]
                    {
                        0x0D, 0x00, 0x21, 0x00, 0x03, 0x3F, 0x09, 0x03,
                        0x4E, 0x00, 0x09, 0x0A, 0x00, 0x00,
                    }),
                    Sl130Protocol.TagInventory => new byte[]
                    {
                        0x2D, 0x00, 0x01, 0x01, 0x03, 0x0C, 0x03, 0x07, 0x26, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x92, 0xD5, 0x0C, 0xE2, 0x00, 0x00, 0x17, 0x29,
                        0x18, 0x01, 0x57, 0x28, 0x80, 0x02, 0x03, 0x0C, 0x03, 0x07, 0x26, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0xB1, 0x89, 0x59,
                    },
                    _ => throw new InvalidOperationException($"Unexpected command 0x{command:X2}"),
                };
                foreach (var value in response) input.Add(value);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    input.CompleteAdding();
                    input.Dispose();
                }
                base.Dispose(disposing);
            }

            public override void Flush() {}
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();

            private static byte[] WithCrc(byte[] frame)
            {
                Crc16.SetCrc16(frame, frame.Length);
                return frame;
            }
        }
    }
}
