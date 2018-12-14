using System;
using maxbl4.RfidDotNet.AlienTech;
using maxbl4.RfidDotNet.AlienTech.Ext;
using maxbl4.RfidDotNet.GenericSerial;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.Tests
{
    public class UnifiedTagStreamFactoryTests
    {
        [Fact]
        public void Should_throw_if_not_registered()
        {
            var factory = new UniversalTagStreamFactory();
            Assert.Throws<ArgumentOutOfRangeException>(() => factory.Create("protocol=alien"));
            Assert.Throws<ArgumentOutOfRangeException>(() => factory.Create("protocol=serial"));
        }
        
        [Fact]
        public void Should_create_instance_from_connection_string()
        {
            var factory = new UniversalTagStreamFactory();
            factory.UseAlienProtocol();
            var stream = factory.Create("protocol=alien; network=localhost");
            stream.ShouldBeOfType<ReconnectingAlienReaderProtocol>();
            
            factory = new UniversalTagStreamFactory();
            factory.UseSerialProtocol();
            stream = factory.Create("protocol=serial; serial=COM4");
            stream.ShouldBeOfType<SerialUnifiedTagStream>();
        }
    }
}