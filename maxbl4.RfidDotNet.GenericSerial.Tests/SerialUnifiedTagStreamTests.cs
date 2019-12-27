using System;
using System.Collections.Generic;
using FluentAssertions;
using maxbl4.Infrastructure;
using Xunit;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    [Collection("Hardware")]
    [Trait("Hardware", "True")]
    public class SerialUnifiedTagStreamTests
    {
        private readonly List<Tag> tags = new List<Tag>();
        private readonly List<Exception> errors = new List<Exception>();
        private readonly List<bool> connected = new List<bool>();
        private readonly List<DateTime> heartbeats = new List<DateTime>();
        
        [SkippableTheory]
        [InlineData(ConnectionType.Serial)]
        [InlineData(ConnectionType.Network)]
        public void Should_receive_serial_tag_stream(ConnectionType connectionType)
        {
            using (var stream = new SerialUnifiedTagStream(TestSettings.Instance.GetConnectionString(connectionType)))
            {
                SubscribeStreams(stream);

                connected.Count.Should().Be(1);
                connected[0].Should().BeFalse();
                tags.Count.Should().Be(0);
                errors.Count.Should().Be(0);

                stream.Start().Wait();
                stream.RFPower(25).Result.Should().Be(25);
                new Timing()
                    .FailureDetails(()=> $"heartbeats.Count = {heartbeats.Count} tags.Count = {tags.Count}")
                    .Expect(() => heartbeats.Count > 2 && tags.Count > 20);
                heartbeats.Should().Contain(x => x > DateTime.UtcNow.AddSeconds(-10));
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