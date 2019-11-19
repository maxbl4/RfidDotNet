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
    public class HardwareTests : ReaderFixture
    {
        private static readonly ILogger Logger = Log.ForContext<HardwareTests>();

        private readonly Subject<Tag> tagStream = new Subject<Tag>();
        private readonly Subject<Exception> errorStream = new Subject<Exception>();

        [Fact]
        public async Task Stream_should_continue_after_keepalive_timeout()
        {
            SetTagListHandlerForKnownTags();
            await Proto.Api.TagStreamKeepAliveTime(1);
            await Proto.Api.AntennaSequence("3");
            var tags = new List<Tag>();
            var msgs = new List<string>();
            var errors = new List<Exception>();
            await Proto.StartTagPolling(tagStream, errorStream);
            tagStream.Subscribe(tags.Add);
            errorStream.Subscribe(errors.Add);
            Proto.TagPoller.UnparsedMessages.Subscribe(msgs.Add);            
            await Proto.Api.AntennaSequence("0");
            await Task.Delay(2000);
            tags.Clear();
            await new Timing().ExpectAsync(() => tags.Count > 100);
            Logger.Debug("{LastSeenTime}", tags.Last().LastSeenTime);
            (DateTime.Now - tags.Last().LastSeenTime).TotalMinutes.ShouldBeLessThan(1);
            (await Proto.Api.AntennaSequence("3")).ShouldBe("3");
            await new Timing().ExpectAsync(() => (DateTime.Now - tags.Last().LastSeenTime).TotalSeconds > 1);
            await Task.Delay(2000);
            tags.Clear();
            await Proto.Api.AntennaSequence("0");
            await new Timing().ExpectAsync(() => tags.Count > 100);
            msgs.ForEach(x => Logger.Error(x));
            msgs.Count.ShouldBe(0);
        }

        [Fact()]
        public async Task Should_disconnect_if_no_keepalives()
        {
            if (Settings.UseHardwareReader) return;
            await new Timing()
                    .Timeout(1500)
                    .Context("Did not get first keepalive")
                    .ExpectAsync(() => (DateTime.Now - Proto.LastKeepalive) < TimeSpan.FromSeconds(1));
            Logger.Debug("First keepalive receive");
            if (Settings.UseHardwareReader)
            {
                var w = Proto.Api.Reboot().Wait(AlienReaderProtocol.DefaultReceiveTimeout); //linux hangs on this
                Logger.Debug("Disabled keepalives (reboot reader command). Return in time: {w}", w);
            }
            else
            {
                Simulator.Client.Logic.KeepaliveEnabled = false;
                Logger.Debug("Disabled keepalives on simulator");
            }

            await new Timing()
                    .Timeout(10000)
                    .Context("Keepalives should stop")
                    .ExpectAsync(() => (DateTime.Now - Proto.LastKeepalive) > TimeSpan.FromSeconds(8));
            Logger.Debug("Keepalives have stopped");
            Proto.IsConnected.ShouldBeFalse();
            Logger.Debug("proto in disconnected");
        }

        [Fact]
        public async Task Heartbeat()
        {
            if (!Settings.UseHardwareReader) return;
            await Proto.Api.HeartbeatTime(2);
            using (var disc = new AlienReaderDiscovery())
            {
                var observed = false;
                disc.Discovery.Subscribe(x => observed = true);
                await new Timing().ExpectAsync(() => observed && disc.Readers.Any());
                var infos = disc.Readers.ToList();
                infos.Count.ShouldBe(1);
                infos[0].IPAddress.ShouldBe(IPEndPoint.Parse(Settings.HardwareReaderAddress).Address);
            }
            await Proto.Api.HeartbeatTime(30);
        }
    }
}