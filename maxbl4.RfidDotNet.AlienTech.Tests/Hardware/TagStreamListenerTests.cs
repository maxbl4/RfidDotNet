using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Enums;
using maxbl4.RfidDotNet.AlienTech.Interfaces;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.AlienTech.TagStream;
using maxbl4.RfidDotNet.Infrastructure;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests.Hardware
{
    [Trait("Hardware", "true")]
    public class TagStreamListenerTests : IDisposable
    {
        private readonly IAlienReaderApi reader;
        private readonly SimulatorListener sim;
        private readonly AlienReaderProtocol proto;
        List<Tag> tags = new List<Tag>();
        List<string> msgs = new List<string>();
        private bool completed = false;
        Subject<Tag> tagStream = new Subject<Tag>();

        public TagStreamListenerTests()
        {
            sim = new SimulatorListener();
            proto = new AlienReaderProtocol(receiveTimeout:int.MaxValue);
            proto.ConnectAndLogin(sim.Host, sim.Port, "alien", "password").Wait(2000).ShouldBeTrue();
            proto.StartTagPolling(tagStream).Wait(2000).ShouldBeTrue();
            reader = proto.Api;
            tagStream.Subscribe(tags.Add, ex => throw ex, () => completed = true);
            proto.TagPoller.UnparsedMessages.Subscribe(msgs.Add);
        }

        [Fact]
        public async Task TagStreamServer_prop()
        {
            var tss = await reader.TagStreamServer(new TagStreamServer(7, 4567));
            tss.AllowedClients.ShouldBe(7);
            tss.Port.ShouldBe(4567);
            tss = await reader.TagStreamServer();
            tss.AllowedClients.ShouldBe(7);
            tss.Port.ShouldBe(4567);
        }

        [Fact]
        public async Task Start_and_stop_stream()
        {
            await reader.AntennaSequence("0");
            (await Timing.StartWait(() => tags.Count > 100)).ShouldBeTrue();
            proto.Dispose();
            completed.ShouldBeFalse();
            msgs.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Check_known_tags()
        {
            var exptectedTagIds = new[]
            {
                "E20000165919004418405CBA",
                "E20000165919006718405C92",
                "E20000165919007818405C7B",
                "E20000165919007718405C83",
                "E20000165919006518405C91"
            };

            await reader.AntennaSequence("0");
            await reader.AcqG2AntennaCombine(false);
            await reader.RFModulation(RFModulation.HS);
            (await Timing.StartWait(() => tags.Count > 500)).ShouldBeTrue();
            proto.Dispose();

            var dict = tags.GroupBy(x => x.TagId).ToDictionary(x => x.Key, x => x.ToList());
            dict.Keys.Intersect(exptectedTagIds).Count().ShouldBe(exptectedTagIds.Length);
            foreach (var tagId in exptectedTagIds)
            {
                dict[tagId].Count.ShouldBeGreaterThan(50);
            }
            msgs.Count.ShouldBe(0);
        }

        public void Dispose()
        {
            proto?.Dispose();
            sim?.Dispose();
        }
    }
}