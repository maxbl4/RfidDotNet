using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using maxbl4.RfidDotNet.Infrastructure;
using Shouldly;
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

                connected.Count.ShouldBe(1);
                connected[0].ShouldBeFalse();
                tags.Count.ShouldBe(0);
                errors.Count.ShouldBe(0);

                stream.Start().Wait();
                stream.RFPower(25).Result.ShouldBe(25);
                new Timing()
                    .FailureDetails(()=> $"heartbeats.Count = {heartbeats.Count} tags.Count = {tags.Count}")
                    .Expect(() => heartbeats.Count > 2 && tags.Count > 20);
                heartbeats.ShouldContain(x => x > DateTime.UtcNow.AddSeconds(-10));
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