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
                QFlags = TagInventoryQFlags.SpecialStrategy|TagInventoryQFlags.ImpinjFastId|TagInventoryQFlags.RequestStatisticsPacket,
                Session = SessionValue.S1,
            }.Serialize();
            TagInventoryParams.BaseSize.ShouldBe(2);
            res.Length.ShouldBe(TagInventoryParams.BaseSize);
            (res[0] & 0b0001_1111).ShouldBe(15);
            ((TagInventoryQFlags)res[0] & TagInventoryQFlags.SpecialStrategy).ShouldBe(TagInventoryQFlags.SpecialStrategy);
            ((TagInventoryQFlags)res[0] & TagInventoryQFlags.ImpinjFastId).ShouldBe(TagInventoryQFlags.ImpinjFastId);
            ((TagInventoryQFlags)res[0] & TagInventoryQFlags.RequestStatisticsPacket).ShouldBe(TagInventoryQFlags.RequestStatisticsPacket);
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