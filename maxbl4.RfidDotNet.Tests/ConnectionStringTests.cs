using System;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.Tests
{
    public class ConnectionStringTests
    {
        [Fact]
        public void Should_set_all_values()
        {
            var cs = ConnectionString.Parse(@"ProtocolType = Alien; TcpHost=localhost;TcpPort=1234 ;
                                   SerialPortName  =  COM4;    SerialBaudRate=57600;QValue=10;Session=3;RFPower=30;
                                   AntennaConfiguration=Antenna1,antenna2;
                                login=Aaa; password= Bbbb;");
            cs.ProtocolType.ShouldBe(ReaderProtocolType.Alien);
            cs.TcpHost.ShouldBe("localhost");
            cs.Login.ShouldBe("Aaa");
            cs.Password.ShouldBe("Bbbb");
            cs.TcpPort.ShouldBe(1234);
            cs.SerialPortName.ShouldBe("COM4");
            cs.SerialBaudRate.ShouldBe(57600);
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
            var cs = ConnectionString.Parse(@"protocoltype=alien");
            cs.ProtocolType.ShouldBe(ReaderProtocolType.Alien);
            cs.IsValid(out var msg).ShouldBeFalse();
            msg.ShouldBe(@"Alien protocol requires TcpHost
Alien protocol requires valid TcpPort");
        }
        
        [Fact]
        public void Should_validate_generic_protocol()
        {
            var cs = ConnectionString.Parse(@"protocoltype=genericSerial");
            cs.ProtocolType.ShouldBe(ReaderProtocolType.GenericSerial);
            cs.IsValid(out var msg).ShouldBeFalse();
            msg.ShouldBe(@"Generic protocol requires SerialPortName or TcpHost");
            cs.SerialPortName = "COM4";
            cs.SerialBaudRate = 0;
            cs.IsValid(out msg).ShouldBeFalse();
            msg.ShouldBe(@"Generic protocol requires SerialBaudRate with SerialPortName");
            cs.SerialPortName = null;
            cs.TcpHost = "host";
            cs.IsValid(out msg).ShouldBeFalse();
            msg.ShouldBe(@"Generic protocol requires TcpPort with TcpHost");
        }
        
        [Fact]
        public void Enum_should_parse_or_values()
        {
            AntennaConfiguration res;
            Enum.TryParse("Antenna1,antenna2", true, out res)
                .ShouldBe(true);
            res.ShouldBe(AntennaConfiguration.Antenna1|AntennaConfiguration.Antenna2);
        }
    }
}