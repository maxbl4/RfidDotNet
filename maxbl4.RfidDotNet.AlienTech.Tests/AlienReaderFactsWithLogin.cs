using System;
using System.Linq;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Enums;
using maxbl4.RfidDotNet.AlienTech.Interfaces;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.Infrastructure;
using Serilog;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests
{
    public class AlienReaderFactsWithLogin : IDisposable
    {
        static readonly ILogger Logger = Log.ForContext<AlienReaderFactsWithLogin>();
        private readonly IAlienReaderApi reader;
        private SimulatorListener sim;
        private AlienReaderProtocol proto;
        private const int baseTimeout = 1000;

        public AlienReaderFactsWithLogin()
        {
            sim = new SimulatorListener();
            proto = new AlienReaderProtocol(baseTimeout, baseTimeout * 2);
            proto.ConnectAndLogin(sim.Host, sim.Port, "alien", "password").Wait(6000).ShouldBeTrue();
            reader = proto.Api;
        }

        [Fact]
        public void Reader_bounce_current_client_when_new_comes()
        {
            Timing.StartWait(() => (DateTime.Now - proto.LastKeepalive).TotalMilliseconds < AlienReaderProtocol.DefaultKeepaliveTimeout, 
                    AlienReaderProtocol.DefaultReceiveTimeout)
                .Result
                .ShouldBeTrue("Did not get first keepalive");
            using (var r2 = new AlienReaderProtocol())
            {
                Logger.Debug("Connecting second client");
                r2.ConnectAndLogin(sim.Host, sim.Port, "alien", "password").Wait(6000).ShouldBeTrue();
                Logger.Debug("Second client connected");
            }
            Logger.Debug("Second client disconnected");
            
            Timing.StartWait(() => (DateTime.Now - proto.LastKeepalive).TotalMilliseconds > AlienReaderProtocol.DefaultKeepaliveTimeout * 2, 
                    AlienReaderProtocol.DefaultReceiveTimeout * 2)
                .Result
                .ShouldBeTrue($"Still getting keepalives {proto.LastKeepalive} {DateTime.Now}");
            Logger.Information("Keepalives stopped");
            Timing.StartWait(() => !proto.IsConnected).Result.ShouldBe(true);
        }

        [Fact]
        public async Task Empty_taglist()
        {
            await reader.AutoModeReset();
            await reader.Clear();
            (await reader.AntennaSequence("3")).ShouldBe("3");
            (await reader.TagList()).ShouldBe(ProtocolMessages.NoTags);
        }

        [Fact]
        public async Task Should_get_keepalives()
        {
            (await Timing.StartWait(() => (DateTime.Now - proto.LastKeepalive) < TimeSpan.FromSeconds(1), 1500))
                .ShouldBeTrue("Did not get first keepalive");
            await Task.Delay(1000);
            (await Timing.StartWait(() => (DateTime.Now - proto.LastKeepalive) < TimeSpan.FromSeconds(1), 1500))
                .ShouldBeTrue("Did not get second keepalive");
        }

        [Fact]
        public async Task Clear_taglist()
        {
            (await reader.Clear()).ShouldBe(ProtocolMessages.TagListClearConfirmation);
        }

        [Fact]
        public async Task Return_taglist()
        {
            await reader.Clear();
            await reader.AntennaSequence("0");
            await reader.RFLevel(180);
            await reader.TagListFormat(ListFormat.Custom);
            await reader.TagListCustomFormat("%k");
            var tagList = await reader.TagList();
            var tags = tagList.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var exptectedTags = new[]
            {
                "E20000165919004418405CBA",
                "E20000165919006718405C92",
                "E20000165919007818405C7B",
                "E20000165919007718405C83",
                "E20000165919006518405C91"
            };
            exptectedTags.Except(tags).Count().ShouldBe(0);
        }

        public void Dispose()
        {
            proto?.Dispose();
            sim?.Dispose();
        }
    }
}