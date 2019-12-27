using FluentAssertions;
using Xunit;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    public class Crc16Tests
    {
        [Fact]
        public void Crc_should_be_set_instead_of_zeroes()
        {
            byte[] example = {0x04, 0x00, 0x4c, 0x3a, 0xd2};
            byte[] test = {0x04, 0x00, 0x4c, 0, 0};
            Crc16.SetCrc16(test);
            test.Should().Equal(example);
        }

        [Fact]
        public void Check_crc_should_return_true_for_examples()
        {
            for (var i = 0; i < SamplesData.AllPackets.Length; i++)
            {
                var e = SamplesData.AllPackets[i];
                Crc16.CheckCrc16(e).Should().BeTrue($"Example {i}");
            }
        }

        [Fact]
        public void Should_set_as_example()
        {
            foreach (var e in SamplesData.AllPackets)
            {
                var test = (byte[])e.Clone();
                test[test.Length - 2] = 0;
                test[test.Length - 1] = 0;
                Crc16.SetCrc16(test);
                test.Should().Equal(e);
            }
        }
        
        [Fact]
        public void Array_clone_test()
        {
            byte[] src = {1, 2, 3};
            byte[] dst = (byte[])src.Clone();
            dst.Should().NotBeSameAs(src);
            dst.Should().Equal(src);
            dst.Length.Should().Be(3);
            dst[0].Should().Be((byte)1);
        }

    }
}