using System;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Infrastructure;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.AlienTech.Tests.Settings;
using Serilog;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests
{
    public class AlienReaderFactsWithLogin : ReaderFixture
    {
        static readonly ILogger Logger = Log.ForContext<AlienReaderFactsWithLogin>();
        
        [Fact]
        public void Reader_bounce_current_client_when_new_comes()
        {
            new Timing()
                .Timeout(AlienReaderProtocol.DefaultReceiveTimeout)
                .Context("Did not get first keepalive")
                .Expect(() => (DateTime.UtcNow - Proto.LastKeepalive).TotalMilliseconds < AlienReaderProtocol.DefaultKeepaliveTimeout);
            using (var r2 = new AlienReaderProtocol())
            {
                Logger.Debug("Connecting second client");
                r2.ConnectAndLogin(Host, Port, "alien", "password").Wait(6000).Should().BeTrue();
                Logger.Debug("Second client connected");
            }
            Logger.Debug("Second client disconnected");
            
            new Timing()
                .Timeout(AlienReaderProtocol.DefaultReceiveTimeout * 2)
                .FailureDetails(() => $"Still getting keepalives {Proto.LastKeepalive} {DateTime.UtcNow}")
                .Expect(() => (DateTime.UtcNow - Proto.LastKeepalive).TotalMilliseconds > AlienReaderProtocol.DefaultKeepaliveTimeout * 2);
            Logger.Information("Keepalives stopped");
            new Timing().Expect(() => !Proto.IsConnected);
        }

        [Fact]
        public async Task Empty_taglist()
        {
            await Proto.Api.AutoModeReset();
            await Proto.Api.Clear();
            (await Proto.Api.AntennaSequence("3")).Should().Be("3");
            (await Proto.Api.TagList()).Should().Be(ProtocolMessages.NoTags);
        }

        [Fact]
        public async Task Should_get_keepalives()
        {
            await new Timing()
                    .Timeout(1500)
                    .Context("Did not get first keepalive")
                    .ExpectAsync(() => (DateTime.UtcNow - Proto.LastKeepalive) < TimeSpan.FromSeconds(1));
            await Task.Delay(1000);
            await new Timing()
                    .Timeout(1500)
                    .Context("Did not get second keepalive")
                    .ExpectAsync(() => (DateTime.UtcNow - Proto.LastKeepalive) < TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Clear_taglist()
        {
            (await Proto.Api.Clear()).Should().Be(ProtocolMessages.TagListClearConfirmation);
        }
    }
}