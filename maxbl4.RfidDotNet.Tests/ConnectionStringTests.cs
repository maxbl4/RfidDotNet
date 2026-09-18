using System;
using System.Net;
using FluentAssertions;
using maxbl4.RfidDotNet.Extensions.Endpoint;
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
            cs.Protocol.Should().Be(ReaderProtocolType.Alien);
            cs.Network.Host.Should().Be("localhost");
            cs.Network.Port.Should().Be(1234);
            cs.Serial.Port.Should().Be("COM4");
            cs.Serial.BaudRate.Should().Be(57600);
            cs.Login.Should().Be("Aaa");
            cs.Password.Should().Be("Bbbb");
            cs.QValue.Should().Be(10);
            cs.Session.Should().Be(3);
            cs.RFPower.Should().Be(30);
            cs.AntennaConfiguration.Should().Be(AntennaConfiguration.Antenna1|AntennaConfiguration.Antenna2);
            cs.IsValid(out var msg).Should().BeTrue();
            msg.Should().BeEmpty();
        }
        
        [Fact]
        public void Should_parse_inventory_duration_into_its_own_property()
        {
            // Regression: InventoryDuration used to be assigned to QValue, so any
            // existing config with InventoryDuration produced QValue out of range 1-16
            // and CreateStream failed with a misleading ArgumentException.
            var cs = ConnectionString.Parse(
                "protocol=Alien;Network=10.0.1.41;RfPower=200;InventoryDuration=500");
            cs.InventoryDuration.Should().Be(500);
            cs.QValue.Should().Be(ConnectionString.DefaultQValue);
            cs.IsValid(out var msg).Should().BeTrue(msg);
        }

        [Fact]
        public void Should_keep_every_parsed_value_in_its_own_property()
        {
            var cs = ConnectionString.Parse(
                "protocol=Serial;Serial=COM4@57600;QValue=7;Session=2;RFPower=210;" +
                "ThermalLimit=55;InventoryDuration=1500;AntennaConfiguration=Antenna3");
            cs.QValue.Should().Be(7);
            cs.Session.Should().Be(2);
            cs.RFPower.Should().Be(210);
            cs.ThermalLimit.Should().Be(55);
            cs.InventoryDuration.Should().Be(1500);
            cs.AntennaConfiguration.Should().Be(AntennaConfiguration.Antenna3);
        }

        [Fact]
        public void Should_validate_inventory_duration_range()
        {
            // The upper bound used to be checked against QValue, so an out of range
            // InventoryDuration passed validation silently.
            var cs = ConnectionString.Parse("protocol=fake;InventoryDuration=30000");
            cs.Protocol = ReaderProtocolType.Alien;
            cs.Network = new DnsEndPoint("host", 23);
            cs.IsValid(out var msg).Should().BeFalse();
            msg.Should().Contain("InventoryDuration must be in range 1-25000 ms, was 30000");
        }

        [Fact]
        public void Should_round_trip_through_to_string()
        {
            var original = ConnectionString.Parse(
                "protocol=Alien;Network=10.0.1.41:23;Login=admin;Password=secret;QValue=7;" +
                "Session=2;RFPower=210;ThermalLimit=55;InventoryDuration=1500;" +
                "AntennaConfiguration=Antenna1,Antenna2");
            var restored = ConnectionString.Parse(original.ToString());

            restored.Protocol.Should().Be(original.Protocol);
            restored.Network.Host.Should().Be(original.Network.Host);
            restored.Network.Port.Should().Be(original.Network.Port);
            restored.Login.Should().Be(original.Login);
            restored.Password.Should().Be(original.Password);
            restored.QValue.Should().Be(original.QValue);
            restored.Session.Should().Be(original.Session);
            restored.RFPower.Should().Be(original.RFPower);
            restored.ThermalLimit.Should().Be(original.ThermalLimit);
            restored.InventoryDuration.Should().Be(original.InventoryDuration);
            // AntennaConfiguration used to be missing from ToString(), so a round trip
            // silently reset it to the default single antenna.
            restored.AntennaConfiguration.Should().Be(original.AntennaConfiguration);
        }

        [Fact]
        public void Should_validate_unknown_protocol()
        {
            var cs = ConnectionString.Parse(@"");
            cs.IsValid(out var msg).Should().BeFalse();
            msg.Should().StartWith("Unknown protocol type");
        }
        
        [Fact]
        public void Should_validate_fake_protocol()
        {
            var cs = ConnectionString.Parse(@"protocol=fake");
            cs.Protocol.Should().Be(ReaderProtocolType.Fake);
            cs.IsValid(out var msg).Should().BeTrue();
            msg.Should().BeEmpty();
        }
        
        [Fact]
        public void Should_validate_alien_protocol()
        {
            var cs = ConnectionString.Parse(@"protocol=alien");
            cs.Protocol.Should().Be(ReaderProtocolType.Alien);
            cs.IsValid(out var msg).Should().BeFalse();
            msg.Should().Be(@"Alien protocol requires Network endpoint");
        }
        
        [Fact]
        public void Should_validate_generic_protocol()
        {
            var cs = ConnectionString.Parse(@"protocol=Serial");
            cs.Protocol.Should().Be(ReaderProtocolType.Serial);
            cs.IsValid(out var msg).Should().BeFalse();
            msg.Should().Be(@"Serial protocol requires Serial or Network endpoint");
            cs.Serial =new SerialEndpoint("COM4", 0);
            cs.IsValid(out msg).Should().BeFalse();
            msg.Should().Be(@"Serial protocol requires valid BaudRate on Serial endpoint");
            cs.Serial = null;
            cs.Network = new DnsEndPoint("host", 0);
            cs.IsValid(out msg).Should().BeFalse();
            msg.Should().Be(@"Serial protocol requires valid Port on Network endpoint");
        }
        
        [Fact]
        public void Should_return_invalid_connection_string_for_null_string()
        {
            var cs = ConnectionString.Parse(null);
            cs.IsValid(out var msg).Should().BeFalse();
        }
        
        [Fact]
        public void Enum_should_parse_or_values()
        {
            AntennaConfiguration res;
            Enum.TryParse("Antenna1,antenna2", true, out res)
                .Should().Be(true);
            res.Should().Be(AntennaConfiguration.Antenna1|AntennaConfiguration.Antenna2);
        }
        
        [Fact]
        public void Should_clone_connection_string()
        {
            var cs1 = new ConnectionString{Protocol = ReaderProtocolType.Alien, Network = new DnsEndPoint("host", 111)};
            var cs2 = cs1.Clone();
            cs2.Should().NotBeSameAs(cs1);
            cs2.Protocol.Should().Be(ReaderProtocolType.Alien);
            cs2.Network.Host.Should().Be("host");
            cs2.Network.Port.Should().Be(111);
        }
        
        [Fact]
        public void Should_parse_dns_endpoint()
        {
            var ep = "host".ParseDnsEndPoint(100);
            ep.Host.Should().Be("host");
            ep.Port.Should().Be(100);
            ep = "host:201".ParseDnsEndPoint(100);
            ep.Host.Should().Be("host");
            ep.Port.Should().Be(201);
        }
    }
}