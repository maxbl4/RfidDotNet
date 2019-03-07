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
        List<Exception> errors = new List<Exception>();
        List<string> msgs = new List<string>();
        private bool completed = false;
        Subject<Tag> tagStream = new Subject<Tag>();
        Subject<Exception> errorStream = new Subject<Exception>();

        public TagStreamListenerTests()
        {
            sim = new SimulatorListener();
            proto = new AlienReaderProtocol(receiveTimeout:int.MaxValue);
            proto.ConnectAndLogin(sim.Host, sim.Port, "alien", "password").Wait(2000).ShouldBeTrue();
            proto.StartTagPolling(tagStream, errorStream).Wait(2000).ShouldBeTrue();
            reader = proto.Api;
            tagStream.Subscribe(tags.Add, ex => throw ex, () => completed = true);
            errorStream.Subscribe(errors.Add);
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
            errors.Count.ShouldBe(0);
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
                "E20000165919006518405C91",
                "0307260000000000000090B1",
                "030726000000000000009213",
                "0307260000000000000092D5",
            };

            await reader.AntennaSequence("0");
            await reader.RFAttenuation(50);
            await reader.AcqG2AntennaCombine(false);
            (await Timing.StartWait(() => tags.Count > 500)).ShouldBeTrue();
            proto.Dispose();

            var dict = tags.GroupBy(x => x.TagId).ToDictionary(x => x.Key, x => x.ToList());
            var foundTags = dict.Keys.Intersect(exptectedTagIds).ToList();
            foundTags.Count.ShouldBeGreaterThanOrEqualTo(3);
            foreach (var tagId in foundTags)
            {
                dict[tagId].Count.ShouldBeGreaterThan(50);
            }
            msgs.Count.ShouldBe(0);
            errors.Count.ShouldBe(0);
        }
        
        [Fact]
        public async Task Recover_from_tags_subscriber_error()
        {
            proto.Dispose();
            tagStream = new Subject<Tag>();
            var p = new AlienReaderProtocol(receiveTimeout:int.MaxValue);
            p.ConnectAndLogin(sim.Host, sim.Port, "alien", "password").Wait(2000).ShouldBeTrue();
            p.StartTagPolling(tagStream, errorStream).Wait(2000).ShouldBeTrue();
            var doThrow = false;
            var badSubscriber = tagStream.Subscribe(x =>
            {
                if (doThrow) throw new Exception();
            });
            tagStream.Subscribe(tags.Add, ex => throw ex, () => completed = true);
            
            await p.Api.AntennaSequence("0");
            (await Timing.StartWait(() => tags.Count > 100)).ShouldBeTrue();
            errors.Count.ShouldBe(0);
            doThrow = true;
            
            (await Timing.StartWait(() => errors.Count > 0)).ShouldBeTrue();
            errors[0].ShouldBeOfType<Exception>();
            tags.Clear();
            (await Timing.StartWait(() => tags.Count > 0, 2000)).ShouldBeFalse();
            badSubscriber.Dispose();
            (await Timing.StartWait(() => tags.Count > 100)).ShouldBeTrue();
        }

        public void Dispose()
        {
            proto?.Dispose();
            sim?.Dispose();
        }
    }
}