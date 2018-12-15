using System;
using System.Net;
using maxbl4.RfidDotNet.Ext;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.Tests
{
    public class ConnectionStringTests
    {
        [Fact]
        public void Should_set_all_values()
        {
            var cs = ConnectionString.Parse(@"Protocol = Alien; Network=localhost:1234;
                                   Serial  =  COM4@57600;;QValue=10;Session=3;RFPower=30;
                                   AntennaConfiguration=Antenna1,antenna2;
                                login=Aaa; password= Bbbb;");
            cs.Protocol.ShouldBe(ReaderProtocolType.Alien);
            cs.Network.Host.ShouldBe("localhost");
            cs.Network.Port.ShouldBe(1234);
            cs.Serial.Port.ShouldBe("COM4");
            cs.Serial.BaudRate.ShouldBe(57600);
            cs.Login.ShouldBe("Aaa");
            cs.Password.ShouldBe("Bbbb");
            cs.QValue.ShouldBe(10);
            cs.Session.ShouldBe(3);
            cs.RFPower.ShouldBe(30);
            cs.AntennaConfiguration.ShouldBe(AntennaConfiguration.Antenna1|AntennaConfiguration.Antenna2);
            cs.IsValid(out var msg).ShouldBeTrue();
            msg.ShouldBeEmpty();
        }
        
        [Fact]
        public void Should_validate_unknown_protocol()
        {
            var cs = ConnectionString.Parse(@"");
            cs.IsValid(out var msg).ShouldBeFalse();
            msg.ShouldStartWith("Unknown protocol type");
        }
        
        [Fact]
        public void Should_validate_alien_protocol()
        {
            var cs = ConnectionString.Parse(@"protocol=alien");
            cs.Protocol.ShouldBe(ReaderProtocolType.Alien);
            cs.IsValid(out var msg).ShouldBeFalse();
            msg.ShouldBe(@"Alien protocol requires Network endpoint");
        }
        
        [Fact]
        public void Should_validate_generic_protocol()
        {
            var cs = ConnectionString.Parse(@"protocol=Serial");
            cs.Protocol.ShouldBe(ReaderProtocolType.Serial);
            cs.IsValid(out var msg).ShouldBeFalse();
            msg.ShouldBe(@"Serial protocol requires Serial or Network endpoint");
            cs.Serial =new SerialEndpoint("COM4", 0);
            cs.IsValid(out msg).ShouldBeFalse();
            msg.ShouldBe(@"Serial protocol requires valid BaudRate on Serial endpoint");
            cs.Serial = null;
            cs.Network = new DnsEndPoint("host", 0);
            cs.IsValid(out msg).ShouldBeFalse();
            msg.ShouldBe(@"Serial protocol requires valid Port on Network endpoint");
        }
        
        [Fact]
        public void Should_return_invalid_connection_string_for_null_string()
        {
            var cs = ConnectionString.Parse(null);
            cs.IsValid(out var msg).ShouldBeFalse();
        }
        
        [Fact]
        public void Enum_should_parse_or_values()
        {
            AntennaConfiguration res;
            Enum.TryParse("Antenna1,antenna2", true, out res)
                .ShouldBe(true);
            res.ShouldBe(AntennaConfiguration.Antenna1|AntennaConfiguration.Antenna2);
        }
        
        [Fact]
        public void Should_clone_connection_string()
        {
            var cs1 = new ConnectionString{Protocol = ReaderProtocolType.Alien, Network = new DnsEndPoint("host", 111)};
            var cs2 = cs1.Clone();
            cs2.ShouldNotBeSameAs(cs1);
            cs2.Protocol.ShouldBe(ReaderProtocolType.Alien);
            cs2.Network.Host.ShouldBe("host");
            cs2.Network.Port.ShouldBe(111);
        }
        
        [Fact]
        public void Should_parse_dns_endpoint()
        {
            var ep = "host".ParseDnsEndPoint(100);
            ep.Host.ShouldBe("host");
            ep.Port.ShouldBe(100);
            ep = "host:201".ParseDnsEndPoint(100);
            ep.Host.ShouldBe("host");
            ep.Port.ShouldBe(201);
        }
    }
}