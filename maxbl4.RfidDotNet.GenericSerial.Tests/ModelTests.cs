using System;
using maxbl4.RfidDotNet.GenericSerial.Model;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    public class ModelTests
    {
        [Fact]
        public void TagInventoryParams_should_serialize_correctly()
        {
            var res = new TagInventoryParams
            {
                QValue = 15,
                QFlags = TagInventoryQFlags.SpecialStrategy|TagInventoryQFlags.ImpinjFastId|TagInventoryQFlags.RequestStatisticsPacket,
                Session = SessionValue.S1,
                MaskMemoryArea = MaskMemoryAreaType.User,
                MaskAddress = 0x1234,
                TIDAddress = 5,
                TIDLength = 6
            }.Serialize();
            TagInventoryParams.BaseSize.ShouldBe(8);
            res.Length.ShouldBe(TagInventoryParams.BaseSize);
            (res[0] & 0b0001_1111).ShouldBe(15);
            ((TagInventoryQFlags)res[0] & TagInventoryQFlags.SpecialStrategy).ShouldBe(TagInventoryQFlags.SpecialStrategy);
            ((TagInventoryQFlags)res[0] & TagInventoryQFlags.ImpinjFastId).ShouldBe(TagInventoryQFlags.ImpinjFastId);
            ((TagInventoryQFlags)res[0] & TagInventoryQFlags.RequestStatisticsPacket).ShouldBe(TagInventoryQFlags.RequestStatisticsPacket);
            ((SessionValue)res[1]).ShouldBe(SessionValue.S1);
            ((MaskMemoryAreaType)res[2]).ShouldBe(MaskMemoryAreaType.User);
            res[3].ShouldBe((byte)0x34);
            res[4].ShouldBe((byte)0x12);
            res[5].ShouldBe((byte)0);
            res[6].ShouldBe((byte)5);
            res[7].ShouldBe((byte)6);
        }
        
        [Fact]
        public void TagInventoryParams_with_mask_data_should_serialize_correctly()
        {
            var res = new TagInventoryParams
            {
                QValue = 15,
                QFlags = TagInventoryQFlags.SpecialStrategy|TagInventoryQFlags.ImpinjFastId|TagInventoryQFlags.RequestStatisticsPacket,
                Session = SessionValue.S1,
                MaskMemoryArea = MaskMemoryAreaType.User,
                MaskLength = 16,
                MaskData = new byte[]{55,66},
                MaskAddress = 0x1234,
                TIDAddress = 5,
                TIDLength = 6
            }.Serialize();
            res.Length.ShouldBe(TagInventoryParams.BaseSize + 2);
            (res[0] & 0b0001_1111).ShouldBe(15);
            ((TagInventoryQFlags)res[0] & TagInventoryQFlags.SpecialStrategy).ShouldBe(TagInventoryQFlags.SpecialStrategy);
            ((TagInventoryQFlags)res[0] & TagInventoryQFlags.ImpinjFastId).ShouldBe(TagInventoryQFlags.ImpinjFastId);
            ((TagInventoryQFlags)res[0] & TagInventoryQFlags.RequestStatisticsPacket).ShouldBe(TagInventoryQFlags.RequestStatisticsPacket);
            ((SessionValue)res[1]).ShouldBe(SessionValue.S1);
            ((MaskMemoryAreaType)res[2]).ShouldBe(MaskMemoryAreaType.User);
            res[3].ShouldBe((byte)0x34);
            res[4].ShouldBe((byte)0x12);
            res[5].ShouldBe((byte)16);
            res[6].ShouldBe((byte)55);
            res[7].ShouldBe((byte)66);
            res[8].ShouldBe((byte)5);
            res[9].ShouldBe((byte)6);
        }
        
        [Fact]
        public void TagInventoryParams_with_optional_params_should_serialize_correctly()
        {
            var res = new TagInventoryParams
            {
                QValue = 15,
                QFlags = TagInventoryQFlags.SpecialStrategy|TagInventoryQFlags.ImpinjFastId|TagInventoryQFlags.RequestStatisticsPacket,
                Session = SessionValue.S1,
                MaskMemoryArea = MaskMemoryAreaType.User,
                MaskLength = 16,
                MaskData = new byte[]{55,66},
                MaskAddress = 0x1234,
                TIDAddress = 5,
                TIDLength = 6,
                OptionalParams = new TagInventoryOptionalParams(TimeSpan.FromMilliseconds(1500), EPCTarget.B, InventoryAntenna.Antenna2)
            }.Serialize();
            res.Length.ShouldBe(TagInventoryParams.BaseSize + TagInventoryParams.OptionalParamsSize + 2);
            (res[0] & 0b0001_1111).ShouldBe(15);
            ((TagInventoryQFlags)res[0] & TagInventoryQFlags.SpecialStrategy).ShouldBe(TagInventoryQFlags.SpecialStrategy);
            ((TagInventoryQFlags)res[0] & TagInventoryQFlags.ImpinjFastId).ShouldBe(TagInventoryQFlags.ImpinjFastId);
            ((TagInventoryQFlags)res[0] & TagInventoryQFlags.RequestStatisticsPacket).ShouldBe(TagInventoryQFlags.RequestStatisticsPacket);
            ((SessionValue)res[1]).ShouldBe(SessionValue.S1);
            ((MaskMemoryAreaType)res[2]).ShouldBe(MaskMemoryAreaType.User);
            res[3].ShouldBe((byte)0x34);
            res[4].ShouldBe((byte)0x12);
            res[5].ShouldBe((byte)16);
            res[6].ShouldBe((byte)55);
            res[7].ShouldBe((byte)66);
            res[8].ShouldBe((byte)5);
            res[9].ShouldBe((byte)6);
            res[10].ShouldBe((byte)EPCTarget.B);
            res[11].ShouldBe((byte)InventoryAntenna.Antenna2);
            res[12].ShouldBe((byte)15);
        }

        [Fact]
        public void TagInventoryParams_should_throw_if_has_mask_length_but_not_mask_data()
        {
            Assert.Throws<ArgumentNullException>(nameof(TagInventoryParams.MaskData), () => new TagInventoryParams
            {
                MaskLength = 10
            }.Serialize());
        }
        
        [Fact]
        public void TagInventoryParams_mask_length_validation_should_work()
        {
            for (byte i = 1; i <= 8; i++)
            {
                new TagInventoryParams { MaskLength = i, MaskData = new byte[1] }.Serialize();    
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new TagInventoryParams {MaskLength = 9, MaskData = new byte[1]}.Serialize());
            for (byte i = 9; i <= 16; i++)
            {
                new TagInventoryParams { MaskLength = i, MaskData = new byte[2] }.Serialize();    
            }
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new TagInventoryParams {MaskLength = 17, MaskData = new byte[2]}.Serialize());
        }
    }
}