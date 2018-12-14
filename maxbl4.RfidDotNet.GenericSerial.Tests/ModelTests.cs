using System;
using maxbl4.RfidDotNet.GenericSerial.Model;
using maxbl4.RfidDotNet.GenericSerial.Packets;
using Shouldly;
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
            res.ShouldBe(SamplesData.TagInventoryRequest1);
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
            TagInventoryParams.BaseSize.ShouldBe(2);
            res.Length.ShouldBe(TagInventoryParams.BaseSize);
            (res[0] & 0b0001_1111).ShouldBe(15);
            ((QFlags)res[0] & QFlags.SpecialStrategy).ShouldBe(QFlags.SpecialStrategy);
            ((QFlags)res[0] & QFlags.ImpinjFastId).ShouldBe(QFlags.ImpinjFastId);
            ((QFlags)res[0] & QFlags.RequestStatisticsPacket).ShouldBe(QFlags.RequestStatisticsPacket);
            ((SessionValue)res[1]).ShouldBe(SessionValue.S1);
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
            res.ShouldBe(SamplesData.TagInventoryWithBufferRequest1);
        }
    }
}