using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using maxbl4.RfidDotNet.Infrastructure;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    public class SerialUnifiedTagStreamTests
    {
        private readonly List<Tag> tags = new List<Tag>();
        private readonly List<Exception> errors = new List<Exception>();
        private readonly List<bool> connected = new List<bool>();
        
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
                Timing.StartWait(() => tags.Count > 20).Result.ShouldBeTrue("Could not read 100 tags before timeout expired");
            }
        }

        private void SubscribeStreams(IUniversalTagStream stream)
        {
            stream.Connected.Subscribe(connected.Add);
            stream.Tags.Subscribe(tags.Add);
            stream.Errors.Subscribe(errors.Add);
        }
    }
}