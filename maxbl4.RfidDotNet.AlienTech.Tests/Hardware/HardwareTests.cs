using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Interfaces;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.AlienTech.TagStream;
using maxbl4.RfidDotNet.AlienTech.Tests.Settings;
using maxbl4.RfidDotNet.Infrastructure;
using Serilog;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests.Hardware
{
    [Trait("Hardware", "true")]
    public class HardwareTests : IClassFixture<ReaderFixture>
    {
        private readonly ReaderFixture readerFixture;
        private static readonly ILogger Logger = Log.ForContext<HardwareTests>();
        private readonly IAlienReaderApi reader;

        private readonly Subject<Tag> tagStream = new Subject<Tag>();
        private readonly Subject<Exception> errorStream = new Subject<Exception>();

        public HardwareTests(ReaderFixture readerFixture)
        {
            this.readerFixture = readerFixture;
            reader = readerFixture.Proto.Api;
        }

        [Fact]
        public async Task Stream_should_continue_after_keepalive_timeout()
        {
            await reader.TagStreamKeepAliveTime(1);
            await reader.AntennaSequence("3");
            var tags = new List<Tag>();
            var msgs = new List<string>();
            var errors = new List<Exception>();
            await readerFixture.Proto.StartTagPolling(tagStream, errorStream);
            tagStream.Subscribe(tags.Add);
            errorStream.Subscribe(errors.Add);
            readerFixture.Proto.TagPoller.UnparsedMessages.Subscribe(msgs.Add);            
            await reader.AntennaSequence("0");
            await Task.Delay(2000);
            tags.Clear();
            (await Timing.StartWait(() => tags.Count > 100)).ShouldBeTrue();
            Logger.Debug("{LastSeenTime}", tags.Last().LastSeenTime);
            (DateTime.Now - tags.Last().LastSeenTime).TotalMinutes.ShouldBeLessThan(1);
            (await reader.AntennaSequence("3")).ShouldBe("3");
            (await Timing.StartWait(() => (DateTime.Now - tags.Last().LastSeenTime).TotalSeconds > 1)
                ).ShouldBeTrue();
            await Task.Delay(2000);
            tags.Clear();
            await reader.AntennaSequence("0");
            (await Timing.StartWait(() => tags.Count > 100)).ShouldBeTrue();
            msgs.ForEach(x => Logger.Error(x));
            msgs.Count.ShouldBe(0);
        }

        [Fact()]
        public async Task Should_disconnect_if_no_keepalives()
        {
            if (readerFixture.Settings.UseHardwareReader) return;
            (await Timing.StartWait(() => (DateTime.Now - readerFixture.Proto.LastKeepalive) < TimeSpan.FromSeconds(1),
                    1500))
                .ShouldBeTrue("Did not get first keepalive");
            Logger.Debug("First keepalive receive");
            if (readerFixture.Settings.UseHardwareReader)
            {
                var w = reader.Reboot().Wait(AlienReaderProtocol.DefaultReceiveTimeout); //linux hangs on this
                Logger.Debug("Disabled keepalives (reboot reader command). Return in time: {w}", w);
            }
            else
            {
                readerFixture.Simulator.Client.Logic.KeepaliveEnabled = false;
                Logger.Debug("Disabled keepalives on simulator");
            }

            (await Timing.StartWait(() => (DateTime.Now - readerFixture.Proto.LastKeepalive) > TimeSpan.FromSeconds(8),
                    10000))
                .ShouldBeTrue("Keepalives should stop");
            Logger.Debug("Keepalives have stopped");
            readerFixture.Proto.IsConnected.ShouldBeFalse();
            Logger.Debug("proto in disconnected");
        }

        [Fact]
        public async Task Heartbeat()
        {
            await reader.HeartbeatTime(2);
            using (var disc = new AlienReaderDiscovery())
            {
                var observed = false;
                disc.Discovery.Subscribe(x => observed = true);
                (await Timing.StartWait(() => observed && disc.Readers.Any())).ShouldBeTrue();
                var infos = disc.Readers.ToList();
                infos.Count.ShouldBe(1);
                infos[0].IPAddress.ShouldBe(IPAddress.Parse(SimulatorListener.ReaderAddress));
            }
            await reader.HeartbeatTime(30);
        }
    }
}