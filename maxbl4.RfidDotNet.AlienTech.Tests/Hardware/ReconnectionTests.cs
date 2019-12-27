using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Infrastructure;
using maxbl4.RfidDotNet.AlienTech.Tests.Settings;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests.Hardware
{
    public class ReconnectionTests : ReaderFixture
    {
        [Fact]
        public async Task Reconnection_check()
        {
            if (!Settings.UseHardwareReader) return;
            var r = new ReconnectingAlienReaderProtocol(new DnsEndPoint(Host, Port),
                async api => {
                    await api.AntennaSequence("0");
                    await api.TagListAntennaCombine(false);
                    await api.AcqG2AntennaCombine(false);
                    await api.RFAttenuation(100);
                    await api.Time(DateTime.UtcNow);
                });
            r.ReconnectTimeout = 2000;
            var status = new List<bool>();
            var tags = new List<Tag>();
            r.Connected.Subscribe(status.Add);
            r.Tags.Subscribe(tags.Add);
            new Timing().Expect(() => tags.Count > 0);
            status.LastOrDefault().Should().BeTrue();
            await r.Current.Api.Reboot();
            new Timing().Expect(() => (DateTime.UtcNow - tags.Last().LastSeenTime).TotalSeconds > 5);
            tags.Clear();
            r.IsConnected.Should().BeFalse();
            status.Last().Should().BeFalse();
            //Timing.StartWait(() => status.Last(), (int)(AlienReaderProtocol.DefaultReceiveTimeout * 2.5)).Result.Should().BeTrue();
            new Timing().Timeout(90000).Expect(() => tags.Count > 0);
        }
        
        [Fact]
        public async Task Help_should_work()
        {
            if (!Settings.UseHardwareReader) return;
            var proto = new AlienReaderProtocol();
            await proto.ConnectAndLogin(Host, Port, "alien", "password");
            var info = await proto.Api.Command("help");
            info.Should().StartWith("**************************************************************");
            info.Length.Should().BeGreaterThan(500);
        }
    }
}