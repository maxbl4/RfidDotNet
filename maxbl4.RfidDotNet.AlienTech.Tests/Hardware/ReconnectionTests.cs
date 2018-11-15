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
            var r = new ReconnectingAlienReaderProtocol(new IPEndPoint(IPAddress.Parse(sim.Host), sim.Port),
                async api => {
                    await api.AntennaSequence("0");
                    await api.RFModulation(RFModulation.HS);
                    await api.TagListAntennaCombine(false);
                    await api.AcqG2AntennaCombine(false);
                    await api.RFAttenuation(100);
                });
            r.ReconnectTimeout = 2000;
            var status = new List<ConnectionStatus>();
            var tags = new List<Tag>();
            r.ConnectionStatus.Subscribe(status.Add);
            r.Tags.Subscribe(tags.Add);
            Timing.StartWait(() => tags.Count > 0).Result.ShouldBeTrue();
            status.OfType<DisconnectedEvent>().Count().ShouldBe(0);
            status.OfType<FailedToConnect>().Count().ShouldBe(0);
            await r.Current.Api.Reboot();
            Timing.StartWait(() => (DateTimeOffset.UtcNow - tags.Last().LastSeenTime).TotalSeconds > 5).Result.ShouldBeTrue();
            tags.Clear();
            r.IsConnected.ShouldBeFalse();
            status.OfType<DisconnectedEvent>().Count().ShouldBeGreaterThan(0);
            Timing.StartWait(() => status.OfType<FailedToConnect>().Any(), (int)(AlienReaderProtocol.DefaultReceiveTimeout * 2.5)).Result.ShouldBeTrue();
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