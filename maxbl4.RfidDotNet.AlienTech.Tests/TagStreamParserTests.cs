using System;
using maxbl4.RfidDotNet.AlienTech.TagStream;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests
{
    public class TagStreamParserTests
    {
        private readonly TagStreamParser p = new TagStreamParser();

        [Fact]
        public void Unparsed()
        {
            p.Parse("assdf").ShouldBe(TagStreamParserReponse.Failed);
            p.Parse("#ddfsdf").ShouldBe(TagStreamParserReponse.Failed);
            p.Parse("#Time: 2018-01-01 18:30:45.444").ShouldBe(TagStreamParserReponse.Failed);
        }
        
        [Fact]
        public void ReaderInfo()
        {
            p.Parse("#ReaderName: test").ShouldBe(TagStreamParserReponse.ParsedReader);
            p.Reader.ReaderName.ShouldBe("test");
        }
        
        [Fact]
        public void Tag()
        {
            p.Parse("E20000165919007818405C7B;1518784324007;1518784353958;0;-40.3;908").ShouldBe(TagStreamParserReponse.ParsedTag);
            p.Tag.TagId.ShouldBe("E20000165919007818405C7B");
            p.Tag.DiscoveryTime.ShouldBe(new DateTimeOffset(2018,2,16,15,32,04,7, TimeSpan.FromHours(3)));
            p.Tag.LastSeenTime.ShouldBe(new DateTimeOffset(2018,2,16,15,32,33,958, TimeSpan.FromHours(3)));
            p.Tag.Antenna.ShouldBe(0);
            p.Tag.Rssi.ShouldBe(-40.3, 0.1);
            p.Tag.ReadCount.ShouldBe(908);
        }
        
        [Fact]
        public void Tag_with_reader()
        {
            p.Parse("#ReaderName: test").ShouldBe(TagStreamParserReponse.ParsedReader);
            p.Parse("E20000165919007818405C7B;1518784324007;1518784353958;0;-40.3;908").ShouldBe(TagStreamParserReponse.ParsedTag);
            p.Tag.Reader.ShouldNotBeNull();
            p.Tag.Reader.ReaderName.ShouldBe("test");
        }
    }
}