using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using maxbl4.RfidDotNet.AlienTech.Buffers;
using maxbl4.RfidDotNet.AlienTech.Enums;
using maxbl4.RfidDotNet.AlienTech.Ext;
using maxbl4.RfidDotNet.AlienTech.TagStream;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests
{
    public class AlienValueConverterTests
    {
        [Fact]
        public void Object_to_alien_string()
        {
            AlienValueConverter.ToAlienValueString(true).ShouldBe("ON");
            AlienValueConverter.ToAlienValueString(false).ShouldBe("OFF");
            AlienValueConverter.ToAlienValueString(1.3d).ShouldBe("1.3");
            AlienValueConverter.ToAlienValueString(1.3f).ShouldBe("1.3");
            AlienValueConverter.ToAlienValueString("str").ShouldBe("str");
            AlienValueConverter.ToAlienValueString(StringComparison.OrdinalIgnoreCase).ShouldBe("OrdinalIgnoreCase");
            AlienValueConverter.ToAlienValueString(null).ShouldBeNull();
            AlienValueConverter.ToAlienValueString(new IPEndPoint(IPAddress.Parse("10.0.0.59"), 7000)).ShouldBe("10.0.0.59:7000");
            AlienValueConverter.ToAlienValueString(new DateTimeOffset(2018,03,10,23,02,57, TimeSpan.Zero)).ShouldBe("2018/03/10 23:02:57");
        }

        [Fact]
        public void Alien_string_to_object()
        {
            AlienValueConverter.ToStrongType<bool>("ON").ShouldBeTrue();
            AlienValueConverter.ToStrongType<bool>("OFF").ShouldBeFalse();
            AlienValueConverter.ToStrongType<float>("1.3").ShouldBe(1.3f);
            AlienValueConverter.ToStrongType<double>("1.3").ShouldBe(1.3d);
            AlienValueConverter.ToStrongType<string>("str").ShouldBe("str");
            AlienValueConverter.ToStrongType<StringComparison>("OrdinalIgnoreCase")
                .ShouldBe(StringComparison.OrdinalIgnoreCase);
            var ep = AlienValueConverter.ToStrongType<IPEndPoint>("10.0.0.59:7000");
            ep.Address.ShouldBe(IPAddress.Parse("10.0.0.59"));
            ep.Port.ShouldBe(7000);
            AlienValueConverter.ToStrongType<DateTimeOffset>("2018/03/10 23:02:57").ShouldBe(new DateTimeOffset(2018,03,10,23,02,57, TimeSpan.Zero));
        }

        [Fact]
        public void Stream_header()
        {
            var reader = new ReaderInfo();
            reader.ParseLine("#ReaderName: Alien RFID Reader").ShouldBe(true);
            reader.ParseLine("#Hostname: alr-0108e4").ShouldBe(true);
            reader.ParseLine("#IPAddress: 10.0.0.41").ShouldBe(true);
            reader.ParseLine("#IPAddress6: fdaa::aaaa").ShouldBe(true);
            reader.ParseLine("#CommandPort: 23").ShouldBe(true);
            reader.ParseLine("#MACAddress: 00:1B:5F:01:08:E4").ShouldBe(true);
            reader.ParseLine("#Time: 2018/02/16 11:48:21.363").ShouldBe(true);
            reader.ReaderName.ShouldBe("Alien RFID Reader");
            reader.Hostname.ShouldBe("alr-0108e4");
            reader.IPAddress.ToString().ShouldBe("10.0.0.41");
            reader.IPAddress6.ToString().ShouldBe("fdaa::aaaa");
            reader.CommandPort.ShouldBe(23);
            reader.MACAddress.ShouldBe("00:1B:5F:01:08:E4");
            reader.Time.ShouldBe(new DateTimeOffset(2018, 2, 16, 11, 48, 21, 363, TimeSpan.Zero));
            
            reader.ParseLine("#Time: 2018/02/16 16:04:58.744 ").ShouldBe(true);
            reader.Time.ShouldBe(new DateTimeOffset(2018, 2, 16, 16, 04, 58, 744, TimeSpan.Zero));
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
            var ri = ReaderInfo.FromXmlString(doc);
            ri.ReaderName.ShouldBe("Alien RFID Reader");
            ri.IPAddress.ShouldBe(IPAddress.Parse("10.0.0.41"));
            ri.IPAddress6.ShouldBe(IPAddress.Parse("fdaa::aaaa"));
            ri.CommandPort.ShouldBe(23);
            ri.MACAddress.ShouldBe("00:1B:5F:01:08:E4");
            ri.Time.ShouldBeInRange(DateTimeOffset.UtcNow.AddSeconds(-2), DateTimeOffset.UtcNow);
        }
        
        [Fact]
        public void Enum_description()
        {
            AcquireMode.GlobalScroll.ToStringDescriptive().ShouldBe("Global Scroll");
            AcquireMode.Inventory.ToStringDescriptive().ShouldBe("Inventory");
            EnumExt.ParseEnum<AcquireMode>("Global Scroll").ShouldBe(AcquireMode.GlobalScroll);
            EnumExt.ParseEnum<AcquireMode>("Inventory").ShouldBe(AcquireMode.Inventory);
        }

        [Fact]
        public void Process_partial_message()
        {
            var parser = new MessageParser();
            parser.Write("1234").ShouldBe(4);
            parser.Parse(4).ToList().Count.ShouldBe(0);
            parser.Offset.ShouldBe(4);
            parser.BufferLength.ShouldBe(MessageParser.DefaultBufferSize - 4);
            parser.Parse(0).ToList().Count.ShouldBe(0);
            parser.Offset.ShouldBe(4);

            parser.Write("567\n").ShouldBe(4);
            var results = parser.Parse(4).ToList();
            results.Count.ShouldBe(1);
            results[0].ShouldBe("1234567");
            parser.Offset.ShouldBe(0);
            parser.BufferLength.ShouldBe(MessageParser.DefaultBufferSize);
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
            msgs.Count.ShouldBe(1);
            msgs[0].ShouldBe("1234");
        }

        [Fact]
        public void IpAddress_mask()
        {
            var ip = IPAddress.Parse("192.168.1.15");
            ip.Mask(IPAddress.Parse("255.255.255.0"))
                .ShouldBe(IPAddress.Parse("192.168.1.0"));
        }

        [Fact]
        public void Process_partial_message_with_lefover()
        {
            var parser = new MessageParser();

            parser.Write("123\n567").ShouldBe(7);
            var results = parser.Parse(7).ToList();
            results.Count.ShouldBe(1);
            results[0].ShouldBe("123");
            parser.Offset.ShouldBe(3);

            parser.Write("\n");
            parser.Parse(1).Single().ShouldBe("567");
        }

        [Fact]
        public void Tag_parser()
        {
            var tagString = "E20000165919004418405CBA;1518371341633;1518371343641;0;-35.4;66";
            var tag = Tag.Parse(tagString);
            tag.TagId.ShouldBe("E20000165919004418405CBA");
            tag.DiscoveryTime.ShouldBe(DateTimeOffset.Parse("2018-02-11T20:49:01.6330000+3"));
            tag.LastSeenTime.ShouldBe(DateTimeOffset.Parse("2018-02-11T20:49:03.6410000+3"));
            tag.Antenna.ShouldBe(0);
            tag.Rssi.ShouldBe(-35.4, 0.1);
            tag.ReadCount.ShouldBe(66);

            Tag.TryParse(tagString, out tag).ShouldBeTrue();

            tag.TagId.ShouldBe("E20000165919004418405CBA");
            tag.DiscoveryTime.ShouldBe(DateTimeOffset.Parse("2018-02-11T20:49:01.6330000+3"));
            tag.LastSeenTime.ShouldBe(DateTimeOffset.Parse("2018-02-11T20:49:03.6410000+3"));
            tag.Antenna.ShouldBe(0);
            tag.Rssi.ShouldBe(-35.4, 0.1);
            tag.ReadCount.ShouldBe(66);
        }

        [Fact]
        public void Tag_parse_with_junk()
        {
            var tagString = "\0E20000165919004418405CBA;1518371341633;1518371343641;0;-35.4;66\r\n";
            var tag = Tag.Parse(tagString);
            tag.TagId.ShouldBe("E20000165919004418405CBA");
            tag.DiscoveryTime.ShouldBe(DateTimeOffset.Parse("2018-02-11T20:49:01.6330000+3"));
            tag.LastSeenTime.ShouldBe(DateTimeOffset.Parse("2018-02-11T20:49:03.6410000+3"));
            tag.Antenna.ShouldBe(0);
            tag.Rssi.ShouldBe(-35.4, 0.1);
            tag.ReadCount.ShouldBe(66);
        }

        [Fact]
        public void Process_empty_message_one_terminator()
        {
            var parser = new MessageParser();
            parser.Write("\0").ShouldBe(1);
            var results = parser.Parse(1).ToList();
            parser.Offset.ShouldBe(0);
            results.Count.ShouldBe(1);
            results[0].ShouldBe("");

            parser.Write("\0\0").ShouldBe(2);
            results = parser.Parse(2).ToList();
            parser.Offset.ShouldBe(0);
            results.Count.ShouldBe(1);
            results[0].ShouldBe("");
        }

        [Fact]
        public void Process_multiple_messages()
        {
            var parser = new MessageParser();
            parser.Write("12\n34\n56\n").ShouldBe(9);
            
            var results = parser.Parse(9).ToList();
            parser.Offset.ShouldBe(0);
            results.Count.ShouldBe(3);
            results[0].ShouldBe("12");
            results[1].ShouldBe("34");
            results[2].ShouldBe("56");
        }

        [Fact]
        public void Process_multiple_messages_with_multiple_terminators()
        {
            var parser = new MessageParser();
            var msg = "12\r\n34\n\r56\n\0\n78\r\n\0";
            parser.Write(msg).ShouldBe(msg.Length);
            
            var results = parser.Parse(msg.Length).ToList();
            parser.Offset.ShouldBe(0);
            results.Count.ShouldBe(4);
            results[0].ShouldBe("12");
            results[1].ShouldBe("34");
            results[2].ShouldBe("56");
            results[3].ShouldBe("78");
        }

        [Fact]
        public void Process_multiple_messages_with_leftover()
        {
            var parser = new MessageParser();
            parser.Write("12\n34\n56\n89").ShouldBe(11);
            
            var results = parser.Parse(11).ToList();
            parser.Offset.ShouldBe(2);
            results.Count.ShouldBe(3);
            results[0].ShouldBe("12");
            results[1].ShouldBe("34");
            results[2].ShouldBe("56");
            parser.Write("\n");
            parser.Parse(1).Single().ShouldBe("89");
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