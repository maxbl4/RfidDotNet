using System;
using System.Linq;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Enums;
using maxbl4.RfidDotNet.AlienTech.Interfaces;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.AlienTech.Tests.Settings;
using maxbl4.RfidDotNet.Infrastructure;
using Serilog;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests
{
    public class AlienReaderFactsWithLogin : ReaderFixture
    {
        static readonly ILogger Logger = Log.ForContext<AlienReaderFactsWithLogin>();
        
        [Fact]
        public void Reader_bounce_current_client_when_new_comes()
        {
            Timing.StartWait(() => (DateTime.Now - Proto.LastKeepalive).TotalMilliseconds < AlienReaderProtocol.DefaultKeepaliveTimeout, 
                    AlienReaderProtocol.DefaultReceiveTimeout)
                .Result
                .ShouldBeTrue("Did not get first keepalive");
            using (var r2 = new AlienReaderProtocol())
            {
                Logger.Debug("Connecting second client");
                r2.ConnectAndLogin(Host, Port, "alien", "password").Wait(6000).ShouldBeTrue();
                Logger.Debug("Second client connected");
            }
            Logger.Debug("Second client disconnected");
            
            Timing.StartWait(() => (DateTime.Now - Proto.LastKeepalive).TotalMilliseconds > AlienReaderProtocol.DefaultKeepaliveTimeout * 2, 
                    AlienReaderProtocol.DefaultReceiveTimeout * 2)
                .Result
                .ShouldBeTrue($"Still getting keepalives {Proto.LastKeepalive} {DateTime.Now}");
            Logger.Information("Keepalives stopped");
            Timing.StartWait(() => !Proto.IsConnected).Result.ShouldBe(true);
        }

        [Fact]
        public async Task Empty_taglist()
        {
            await Proto.Api.AutoModeReset();
            await Proto.Api.Clear();
            (await Proto.Api.AntennaSequence("3")).ShouldBe("3");
            (await Proto.Api.TagList()).ShouldBe(ProtocolMessages.NoTags);
        }

        [Fact]
        public async Task Should_get_keepalives()
        {
            (await Timing.StartWait(() => (DateTime.Now - Proto.LastKeepalive) < TimeSpan.FromSeconds(1), 1500))
                .ShouldBeTrue("Did not get first keepalive");
            await Task.Delay(1000);
            (await Timing.StartWait(() => (DateTime.Now - Proto.LastKeepalive) < TimeSpan.FromSeconds(1), 1500))
                .ShouldBeTrue("Did not get second keepalive");
        }

        [Fact]
        public async Task Clear_taglist()
        {
            (await Proto.Api.Clear()).ShouldBe(ProtocolMessages.TagListClearConfirmation);
        }

        [Fact]
        public async Task Return_taglist()
        {
            await Proto.Api.Clear();
            await Proto.Api.AntennaSequence("0");
            await Proto.Api.RFLevel(180);
            await Proto.Api.TagListFormat(ListFormat.Custom);
            await Proto.Api.TagListCustomFormat("%k");
            Simulator.VisibleTags = "11\r\n22\r\n33";
            var tagList = await Proto.Api.TagList();
            tagList.ShouldBe(Simulator.VisibleTags);
        }
    }
}