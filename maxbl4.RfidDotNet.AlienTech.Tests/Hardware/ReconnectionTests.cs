using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Enums;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.AlienTech.TagStream;
using maxbl4.RfidDotNet.Infrastructure;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests.Hardware
{
    public class ReconnectionTests : IDisposable
    {
        private SimulatorListener sim;

        public ReconnectionTests()
        {
            sim = new SimulatorListener();
        }
        
        [Fact]
        public async Task Reconnection_check()
        {
            if (!sim.UsePhysicalDevice) return;
            var r = new ReconnectingAlienReaderProtocol(new DnsEndPoint(sim.Host, sim.Port),
                async api => {
                    await api.AntennaSequence("0");
                    await api.TagListAntennaCombine(false);
                    await api.AcqG2AntennaCombine(false);
                    await api.RFAttenuation(100);
                    await api.Time(DateTimeOffset.UtcNow);
                });
            r.ReconnectTimeout = 2000;
            var status = new List<bool>();
            var tags = new List<Tag>();
            r.Connected.Subscribe(status.Add);
            r.Tags.Subscribe(tags.Add);
            Timing.StartWait(() => tags.Count > 0).Result.ShouldBeTrue();
            status.LastOrDefault().ShouldBeTrue();
            await r.Current.Api.Reboot();
            Timing.StartWait(() => (DateTimeOffset.UtcNow - tags.Last().LastSeenTime).TotalSeconds > 5).Result.ShouldBeTrue();
            tags.Clear();
            r.IsConnected.ShouldBeFalse();
            status.Last().ShouldBeFalse();
            //Timing.StartWait(() => status.Last(), (int)(AlienReaderProtocol.DefaultReceiveTimeout * 2.5)).Result.ShouldBeTrue();
            Timing.StartWait(() => tags.Count > 0, 60000).Result.ShouldBeTrue();
        }
        
        [Fact]
        public async Task Help_should_work()
        {
            if (!sim.UsePhysicalDevice) return;
            var proto = new AlienReaderProtocol();
            await proto.ConnectAndLogin(sim.Host, sim.Port, "alien", "password");
            var info = await proto.Api.Command("help");
            info.ShouldStartWith("**************************************************************");
            info.Length.ShouldBeGreaterThan(500);
        }

        public void Dispose()
        {
            sim?.Dispose();
        }
    }
}