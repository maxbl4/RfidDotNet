using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using FluentAssertions;
using FluentAssertions.Extensions;
using maxbl4.Infrastructure.Extensions.EnumExt;
using maxbl4.Infrastructure.Extensions.IPAddressExt;
using maxbl4.RfidDotNet.AlienTech.Buffers;
using maxbl4.RfidDotNet.AlienTech.Enums;
using maxbl4.RfidDotNet.AlienTech.Extensions;
using maxbl4.RfidDotNet.AlienTech.TagStream;
using maxbl4.RfidDotNet.Exceptions;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests
{
    public class AlienValueConverterTests
    {
        [Fact]
        public void Object_to_alien_string()
        {
            AlienValueConverter.ToAlienValueString(true).Should().Be("ON");
            AlienValueConverter.ToAlienValueString(false).Should().Be("OFF");
            AlienValueConverter.ToAlienValueString(1.3d).Should().Be("1.3");
            AlienValueConverter.ToAlienValueString(1.3f).Should().Be("1.3");
            AlienValueConverter.ToAlienValueString("str").Should().Be("str");
            AlienValueConverter.ToAlienValueString(StringComparison.OrdinalIgnoreCase).Should().Be("OrdinalIgnoreCase");
            AlienValueConverter.ToAlienValueString(null).Should().BeNull();
            AlienValueConverter.ToAlienValueString(new IPEndPoint(IPAddress.Parse("10.0.0.59"), 7000)).Should().Be("10.0.0.59:7000");
            AlienValueConverter.ToAlienValueString(new DateTime(2018,03,10,23,02,57, DateTimeKind.Utc)).Should().Be("2018/03/10 23:02:57");
        }

        [Fact]
        public void Alien_string_to_object()
        {
            AlienValueConverter.ToStrongType<bool>("ON").Should().BeTrue();
            AlienValueConverter.ToStrongType<bool>("OFF").Should().BeFalse();
            AlienValueConverter.ToStrongType<float>("1.3").Should().Be(1.3f);
            AlienValueConverter.ToStrongType<double>("1.3").Should().Be(1.3d);
            AlienValueConverter.ToStrongType<string>("str").Should().Be("str");
            AlienValueConverter.ToStrongType<StringComparison>("OrdinalIgnoreCase")
                .Should().Be(StringComparison.OrdinalIgnoreCase);
            var ep = AlienValueConverter.ToStrongType<IPEndPoint>("10.0.0.59:7000");
            ep.Address.Should().Be(IPAddress.Parse("10.0.0.59"));
            ep.Port.Should().Be(7000);
            AlienValueConverter.ToStrongType<DateTime>("2018/03/10 23:02:57").Should().Be(new DateTime(2018,03,10,23,02,57, DateTimeKind.Utc));
        }

        [Fact]
        public void Stream_header()
        {
            var reader = new ReaderInfo();
            reader.ParseLine("#ReaderName: Alien RFID Reader").Should().Be(true);
            reader.ParseLine("#Hostname: alr-0108e4").Should().Be(true);
            reader.ParseLine("#IPAddress: 10.0.0.41").Should().Be(true);
            reader.ParseLine("#IPAddress6: fdaa::aaaa").Should().Be(true);
            reader.ParseLine("#CommandPort: 23").Should().Be(true);
            reader.ParseLine("#MACAddress: 00:1B:5F:01:08:E4").Should().Be(true);
            reader.ParseLine("#Time: 2018/02/16 11:48:21.363").Should().Be(true);
            reader.ReaderName.Should().Be("Alien RFID Reader");
            reader.Hostname.Should().Be("alr-0108e4");
            reader.IPAddress.ToString().Should().Be("10.0.0.41");
            reader.IPAddress6.ToString().Should().Be("fdaa::aaaa");
            reader.CommandPort.Should().Be(23);
            reader.MACAddress.Should().Be("00:1B:5F:01:08:E4");
            reader.Time.Should().Be(new DateTime(2018, 2, 16, 11, 48, 21, 363, DateTimeKind.Utc));
            
            reader.ParseLine("#Time: 2018/02/16 16:04:58.744 ").Should().Be(true);
            reader.Time.Should().Be(new DateTime(2018, 2, 16, 16, 04, 58, 744, DateTimeKind.Utc));
        }
        
        [Fact]
        public void ReaderInfo_from_xml()
        {
            var doc = new XmlDocument();
            doc.LoadXml(@"<Alien-RFID-Reader-Heartbeat>
  <ReaderName>Alien RFID Reader</ReaderName>
  <ReaderType>Alien RFID Tag Reader, Model: ALR-F800-EMA (EN 302.208, 865-867 MHz)</ReaderType>
  <IPAddress>10.0.0.41</IPAddress>
  <IPv6Address>fdaa::aaaa</IPv6Address>
  <CommandPort>23</CommandPort>
  <HeartbeatTime>30</HeartbeatTime>
  <MACAddress>00:1B:5F:01:08:E4</MACAddress>
  <ReaderVersion>17.11.13.00</ReaderVersion>
</Alien-RFID-Reader-Heartbeat>");
            var ri = ReaderInfoParser.FromXmlString(doc);
            ri.ReaderName.Should().Be("Alien RFID Reader");
            ri.IPAddress.Should().Be(IPAddress.Parse("10.0.0.41"));
            ri.IPAddress6.Should().Be(IPAddress.Parse("fdaa::aaaa"));
            ri.CommandPort.Should().Be(23);
            ri.MACAddress.Should().Be("00:1B:5F:01:08:E4");
            ri.Time.Should().BeWithin(2.Seconds()).Before(DateTime.UtcNow);
        }
        
        [Fact]
        public void Enum_description()
        {
            AcquireMode.GlobalScroll.ToStringDescriptive().Should().Be("Global Scroll");
            AcquireMode.Inventory.ToStringDescriptive().Should().Be("Inventory");
            EnumExt.ParseEnum<AcquireMode>("Global Scroll").Should().Be(AcquireMode.GlobalScroll);
            EnumExt.ParseEnum<AcquireMode>("Inventory").Should().Be(AcquireMode.Inventory);
        }

        [Fact]
        public void Process_partial_message()
        {
            var parser = new MessageParser();
            parser.Write("1234").Should().Be(4);
            parser.Parse(4).ToList().Count.Should().Be(0);
            parser.Offset.Should().Be(4);
            parser.BufferLength.Should().Be(MessageParser.DefaultBufferSize - 4);
            parser.Parse(0).ToList().Count.Should().Be(0);
            parser.Offset.Should().Be(4);

            parser.Write("567\n").Should().Be(4);
            var results = parser.Parse(4).ToList();
            results.Count.Should().Be(1);
            results[0].Should().Be("1234567");
            parser.Offset.Should().Be(0);
            parser.BufferLength.Should().Be(MessageParser.DefaultBufferSize);
        }

        [Fact]
        public void Should_throw_when_message_is_too_long()
        {
            var parser = new MessageParser(5);
            parser.Write("12345");
            Assert.Throws<OutOfBufferSpace>(() => parser.Parse(5).ToList());
        }

        [Fact]
        public void Buffer_ending_with_terminator()
        {
            var parser = new MessageParser(5);
            parser.Write("1234\0");
            var msgs = parser.Parse(5).ToList();
            msgs.Count.Should().Be(1);
            msgs[0].Should().Be("1234");
        }

        [Fact]
        public void IpAddress_mask()
        {
            var ip = IPAddress.Parse("192.168.1.15");
            ip.Mask(IPAddress.Parse("255.255.255.0"))
                .Should().Be(IPAddress.Parse("192.168.1.0"));
        }

        [Fact]
        public void Process_partial_message_with_lefover()
        {
            var parser = new MessageParser();

            parser.Write("123\n567").Should().Be(7);
            var results = parser.Parse(7).ToList();
            results.Count.Should().Be(1);
            results[0].Should().Be("123");
            parser.Offset.Should().Be(3);

            parser.Write("\n");
            parser.Parse(1).Single().Should().Be("567");
        }

        [Fact]
        public void TagParser_SerializeRoundtrip()
        {
            var tagString = "E20000165919004418405CBA;1518371341633;1518371343641;0;-35.4;66";
            TagParser.Parse(tagString).ToCustomFormatString().Should().Be(tagString);
        }

        [Fact]
        public void Tag_parser()
        {
            var tagString = "E20000165919004418405CBA;1518371341633;1518371343641;0;-35.4;66";
            var tag = TagParser.Parse(tagString);
            tag.TagId.Should().Be("E20000165919004418405CBA");
            tag.DiscoveryTime.Should().Be(DateTime.Parse("2018-02-11T20:49:01.6330000+3").ToUniversalTime());
            tag.LastSeenTime.Should().Be(DateTime.Parse("2018-02-11T20:49:03.6410000+3").ToUniversalTime());
            tag.Antenna.Should().Be(0);
            tag.Rssi.Should().BeApproximately(-35.4, 0.1);
            tag.ReadCount.Should().Be(66);

            TagParser.TryParse(tagString, out tag).Should().BeTrue();

            tag.TagId.Should().Be("E20000165919004418405CBA");
            tag.DiscoveryTime.Should().Be(DateTime.Parse("2018-02-11T20:49:01.6330000+3").ToUniversalTime());
            tag.LastSeenTime.Should().Be(DateTime.Parse("2018-02-11T20:49:03.6410000+3").ToUniversalTime());
            tag.Antenna.Should().Be(0);
            tag.Rssi.Should().BeApproximately(-35.4, 0.1);
            tag.ReadCount.Should().Be(66);
        }
        
        [Fact]
        public void Tag_parse_with_junk()
        {
            var tagString = "\0E20000165919004418405CBA;1518371341633;1518371343641;0;-35.4;66\r\n";
            var tag = TagParser.Parse(tagString);
            tag.TagId.Should().Be("E20000165919004418405CBA");
            tag.DiscoveryTime.Should().Be(DateTime.Parse("2018-02-11T20:49:01.6330000+3").ToUniversalTime());
            tag.LastSeenTime.Should().Be(DateTime.Parse("2018-02-11T20:49:03.6410000+3").ToUniversalTime());
            tag.Antenna.Should().Be(0);
            tag.Rssi.Should().BeApproximately(-35.4, 0.1);
            tag.ReadCount.Should().Be(66);
        }

        [Fact]
        public void Process_empty_message_one_terminator()
        {
            var parser = new MessageParser();
            parser.Write("\0").Should().Be(1);
            var results = parser.Parse(1).ToList();
            parser.Offset.Should().Be(0);
            results.Count.Should().Be(1);
            results[0].Should().Be("");

            parser.Write("\0\0").Should().Be(2);
            results = parser.Parse(2).ToList();
            parser.Offset.Should().Be(0);
            results.Count.Should().Be(1);
            results[0].Should().Be("");
        }

        [Fact]
        public void Process_multiple_messages()
        {
            var parser = new MessageParser();
            parser.Write("12\n34\n56\n").Should().Be(9);
            
            var results = parser.Parse(9).ToList();
            parser.Offset.Should().Be(0);
            results.Count.Should().Be(3);
            results[0].Should().Be("12");
            results[1].Should().Be("34");
            results[2].Should().Be("56");
        }

        [Fact]
        public void Process_multiple_messages_with_multiple_terminators()
        {
            var parser = new MessageParser();
            var msg = "12\r\n34\n\r56\n\0\n78\r\n\0";
            parser.Write(msg).Should().Be(msg.Length);
            
            var results = parser.Parse(msg.Length).ToList();
            parser.Offset.Should().Be(0);
            results.Count.Should().Be(4);
            results[0].Should().Be("12");
            results[1].Should().Be("34");
            results[2].Should().Be("56");
            results[3].Should().Be("78");
        }

        [Fact]
        public void Process_multiple_messages_with_leftover()
        {
            var parser = new MessageParser();
            parser.Write("12\n34\n56\n89").Should().Be(11);
            
            var results = parser.Parse(11).ToList();
            parser.Offset.Should().Be(2);
            results.Count.Should().Be(3);
            results[0].Should().Be("12");
            results[1].Should().Be("34");
            results[2].Should().Be("56");
            parser.Write("\n");
            parser.Parse(1).Single().Should().Be("89");
        }
    }

    static class MessageSplitterExt
    {
        public static int Write(this MessageParser m, string data)
        {
            var bytes = Encoding.ASCII.GetBytes(data);
            Array.Copy(bytes, 0, m.Buffer, m.Offset, bytes.Length);
            return bytes.Length;
        }
    }
}