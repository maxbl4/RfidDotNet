using System;
using maxbl4.RfidDotNet.AlienTech.Ext;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests
{
    public class UnifiedTagStreamFactoryTests
    {
        [Fact]
        public void Should_throw_if_not_registered()
        {
            var factory = new UniversalTagStreamFactory();
            Assert.Throws<ArgumentOutOfRangeException>(() => factory.Create("protocolType=alien"));
        }
        
        [Fact]
        public void Should_create_instance_from_connection_string()
        {
            var factory = new UniversalTagStreamFactory();
            factory.UseAlienProtocol();
            var stream = factory.Create("protocolType=alien; tcphost=localhost; tcpport=25");
            stream.ShouldBeOfType<ReconnectingAlienReaderProtocol>();
        }
    }
}