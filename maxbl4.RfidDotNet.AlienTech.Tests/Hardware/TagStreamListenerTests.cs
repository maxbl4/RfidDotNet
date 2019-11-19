using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Enums;
using maxbl4.RfidDotNet.AlienTech.Interfaces;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.AlienTech.TagStream;
using maxbl4.RfidDotNet.AlienTech.Tests.Settings;
using maxbl4.RfidDotNet.Infrastructure;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests.Hardware
{
    public class TagStreamListenerTests : ReaderFixture
    {
        private readonly List<Tag> tags = new List<Tag>();
        private readonly List<Exception> errors = new List<Exception>();
        private readonly List<string> msgs = new List<string>();
        private bool completed = false;
        private Subject<Tag> tagStream = new Subject<Tag>();
        private readonly Subject<Exception> errorStream = new Subject<Exception>();

        public TagStreamListenerTests()
        {
            SetTagListHandlerForKnownTags();
            
            Proto.StartTagPolling(tagStream, errorStream).Wait(2000).ShouldBeTrue();
            tagStream.Subscribe(tags.Add, ex => throw ex, () => completed = true);
            errorStream.Where(x => !(x is ObjectDisposedException)).Subscribe(errors.Add);
            Proto.TagPoller.UnparsedMessages.Subscribe(msgs.Add);
        }

        [Fact]
        public async Task TagStreamServer_prop()
        {
            var tss = await Proto.Api.TagStreamServer(new TagStreamServer(7, 4567));
            tss.AllowedClients.ShouldBe(7);
            tss.Port.ShouldBe(4567);
            tss = await Proto.Api.TagStreamServer();
            tss.AllowedClients.ShouldBe(7);
            tss.Port.ShouldBe(4567);
        }

        [Fact]
        public async Task Start_and_stop_stream()
        {
            await Proto.Api.AntennaSequence("0");
            await new Timing().ExpectAsync(() => tags.Count > 100);
            Proto.Dispose();
            completed.ShouldBeFalse();
            msgs.Count.ShouldBe(0);
            errors.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Check_known_tags()
        {
            await Proto.Api.AntennaSequence("0");
            await Proto.Api.RFAttenuation(50);
            await Proto.Api.AcqG2AntennaCombine(false);
            await new Timing().ExpectAsync(() => tags.Count > 500);
            Proto.Dispose();

            var dict = tags.GroupBy(x => x.TagId).ToDictionary(x => x.Key, x => x.ToList());
            var foundTags = dict.Keys.Intersect(Settings.KnownTagIds).ToList();
            foundTags.Count.ShouldBeGreaterThanOrEqualTo(3);
            foreach (var tagId in foundTags)
            {
                dict[tagId].Count.ShouldBeGreaterThan(50);
            }
            msgs.Count.ShouldBe(0);
            if (errors.Count > 0)
                errors[0].ToString().ShouldBeNull();
            errors.Count.ShouldBe(0);
        }
        
        [Fact]
        public async Task Recover_from_tags_subscriber_error()
        {
            Proto.Dispose();
            tagStream = new Subject<Tag>();
            var p = new AlienReaderProtocol(receiveTimeout:int.MaxValue);
            p.ConnectAndLogin(Host, Port, "alien", "password").Wait(2000).ShouldBeTrue();
            p.StartTagPolling(tagStream, errorStream).Wait(2000).ShouldBeTrue();
            var doThrow = false;
            var badSubscriber = tagStream.Subscribe(x =>
            {
                if (doThrow) throw new Exception();
            });
            tagStream.Subscribe(tags.Add, ex => throw ex, () => completed = true);
            
            await p.Api.AntennaSequence("0");
            await new Timing().ExpectAsync(() => tags.Count > 100);
            errors.Count.ShouldBe(0);
            doThrow = true;
            
            await new Timing().ExpectAsync(() => errors.Count > 0);
            errors[0].ShouldBeOfType<Exception>();
            tags.Clear();
            (await new Timing().Timeout(2000).WaitAsync(() => tags.Count > 0)).ShouldBeFalse();
            badSubscriber.Dispose();
            await new Timing().ExpectAsync(() => tags.Count > 100);
        }
    }
}