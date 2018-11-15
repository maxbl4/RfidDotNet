using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    public class Crc16Tests
    {
        readonly byte[][] examples =
        {
            new byte[]{ 0x04, 0x00, 0x4c, 0x3a, 0xd2 },
            new byte[]{ 0x09, 0x00, 0x4c, 0x00, 0x17, 0x43, 0x90, 0x15, 0x49, 0xc0 },
            new byte[]{ 0x04, 0x00, 0x21, 0xd9, 0x6a},
            new byte[]{ 0x11, 0x00, 0x21, 0x00, 0x03, 0x01, 0x10, 0x02, 0x31, 0x80, 0x1a, 0x03, 0x01, 0x00, 0x00, 0x00, 0xaf, 0xbf},
            new byte[]{ 0x05, 0x00, 0x40, 0x01, 0x99, 0x3a},
            new byte[]{ 0x05, 0x00, 0x40, 0x00, 0x10, 0x2b},
        };

        [Fact]
        public void Crc_should_be_set_instead_of_zeroes()
        {
            byte[] example = {0x04, 0x00, 0x4c, 0x3a, 0xd2};
            byte[] test = {0x04, 0x00, 0x4c, 0, 0};
            Crc16.SetCrc16(test);
            test.ShouldBe(example);
        }

        [Fact]
        public void Check_crc_should_return_true_for_examples()
        {
            for (var i = 0; i < examples.Length; i++)
            {
                var e = examples[i];
                Crc16.CheckCrc16(e).ShouldBeTrue($"Example {i}");
            }
        }

        [Fact]
        public void Should_set_as_example()
        {
            foreach (var e in examples)
            {
                var test = (byte[])e.Clone();
                test[test.Length - 2] = 0;
                test[test.Length - 1] = 0;
                Crc16.SetCrc16(test);
                test.ShouldBe(e);
            }
        }
        
        
        [Fact]
        public void Array_clone_test()
        {
            byte[] src = {1, 2, 3};
            byte[] dst = (byte[])src.Clone();
            dst.ShouldNotBeSameAs(src);
            dst.ShouldBe(src);
            dst.Length.ShouldBe(3);
            dst[0].ShouldBe((byte)1);
        }
    }
}