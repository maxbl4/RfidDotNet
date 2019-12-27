using System;
using FluentAssertions;
using maxbl4.RfidDotNet.GenericSerial.Model;
using maxbl4.RfidDotNet.GenericSerial.Packets;
using Xunit;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    public class ModelTests
    {
        [Fact]
        public void TagInventoryParams_should_serialize_as_example()
        {
            var res = CommandDataPacket.TagInventory(ReaderCommand.TagInventory,
                new TagInventoryParams
            {
                QValue = 4,
                OptionalParams = new TagInventoryOptionalParams(TimeSpan.FromMilliseconds(1100))
            }).Serialize();
            res.Should().Equal(SamplesData.TagInventoryRequest1);
        }

        [Fact]
        public void TagInventoryParams_should_serialize_correctly()
        {
            var res = new TagInventoryParams
            {
                QValue = 15,
                QFlags = QFlags.SpecialStrategy|QFlags.ImpinjFastId|QFlags.RequestStatisticsPacket,
                Session = SessionValue.S1,
            }.Serialize();
            TagInventoryParams.BaseSize.Should().Be(2);
            res.Length.Should().Be(TagInventoryParams.BaseSize);
            (res[0] & 0b0001_1111).Should().Be(15);
            ((QFlags)res[0] & QFlags.SpecialStrategy).Should().Be(QFlags.SpecialStrategy);
            ((QFlags)res[0] & QFlags.ImpinjFastId).Should().Be(QFlags.ImpinjFastId);
            ((QFlags)res[0] & QFlags.RequestStatisticsPacket).Should().Be(QFlags.RequestStatisticsPacket);
            ((SessionValue)res[1]).Should().Be(SessionValue.S1);
        }
        
        [Fact]
        public void TagInventoryParamsWithBuffer_should_serialize_as_example()
        {
            var res = CommandDataPacket.TagInventory(ReaderCommand.TagInventoryWithMemoryBuffer,
                new TagInventoryParams
                {
                    QValue = 4,
                    Session = SessionValue.S1,
                    OptionalParams = new TagInventoryOptionalParams(TimeSpan.FromMilliseconds(300))
                }).Serialize();
            res.Should().Equal(SamplesData.TagInventoryWithBufferRequest1);
        }

        [Fact]
        public void Should_check_type_of_connection_string()
        {
            new SerialConnectionString(ConnectionString.Parse("protocol=Serial;Serial=COM4"))
                .Type.Should().Be(ConnectionType.Serial);
            new SerialConnectionString(ConnectionString.Parse("protocol=Serial;Network=host"))
                .Type.Should().Be(ConnectionType.Network);
            new SerialConnectionString(ConnectionString.Parse("protocol=Alien;Network=host"))
                .Type.Should().Be(ConnectionType.None);
        }
    }
}