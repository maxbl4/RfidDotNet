using System;
using FluentAssertions;
using maxbl4.RfidDotNet.AlienTech.TagStream;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests
{
    public class TagStreamParserTests
    {
        private readonly TagStreamParser p = new();

        [Fact]
        public void Unparsed()
        {
            p.Parse("assdf").Should().Be(TagStreamParserReponse.Failed);
            p.Parse("#ddfsdf").Should().Be(TagStreamParserReponse.Failed);
            p.Parse("#Time: 2018-01-01 18:30:45.444").Should().Be(TagStreamParserReponse.Failed);
        }
        
        [Fact]
        public void ReaderInfo()
        {
            p.Parse("#ReaderName: test").Should().Be(TagStreamParserReponse.ParsedReader);
            p.Reader.ReaderName.Should().Be("test");
        }
        
        [Fact]
        public void Tag()
        {
            p.Parse("E20000165919007818405C7B;1518784324007;1518784353958;0;-40.3;908").Should().Be(TagStreamParserReponse.ParsedTag);
            p.Tag.TagId.Should().Be("E20000165919007818405C7B");
            p.Tag.DiscoveryTime.Should().Be(new DateTime(2018,2,16,12,32,04,7, DateTimeKind.Utc));
            p.Tag.LastSeenTime.Should().Be(new DateTime(2018,2,16,12,32,33,958, DateTimeKind.Utc));
            p.Tag.Antenna.Should().Be(0);
            p.Tag.Rssi.Should().BeApproximately(-40.3, 0.1);
            p.Tag.ReadCount.Should().Be(908);
        }
        
        [Fact]
        public void Tag_with_reader()
        {
            p.Parse("#ReaderName: test").Should().Be(TagStreamParserReponse.ParsedReader);
            p.Parse("E20000165919007818405C7B;1518784324007;1518784353958;0;-40.3;908").Should().Be(TagStreamParserReponse.ParsedTag);
            p.Tag.Reader.Should().NotBeNull();
            p.Tag.Reader.ReaderName.Should().Be("test");
        }
    }
}