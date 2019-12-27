using System.IO;
using System.Linq;
using FluentAssertions;
using maxbl4.RfidDotNet.GenericSerial.Buffers;
using Xunit;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    public class MessageParserTests
    {
        [Fact]
        public void Test_zero_data()
        {
            var ms = new MemoryStream();
            var resp = MessageParser.ReadPacket(ms).Result;
            resp.Success.Should().BeFalse();
            resp.ResultType.Should().Be(PacketResultType.Timeout);
        }
        
        [Fact]
        public void Test_less_data_than_expected()
        {
            var ms = new MemoryStream(SamplesData.Response1.Take(SamplesData.Response1.Length - 1).ToArray());
            var resp = MessageParser.ReadPacket(ms).Result;
            resp.Success.Should().BeFalse();
            resp.ResultType.Should().Be(PacketResultType.WrongSize);
        }
        
        [Fact]
        public void Test_response1()
        {
            var ms = new MemoryStream(SamplesData.Response1);
            var resp = MessageParser.ReadPacket(ms).Result;
            resp.Success.Should().BeTrue();
            resp.ResultType.Should().Be(PacketResultType.Success);
            resp.Data.Should().Equal(SamplesData.Response1);
        }
        
        [Fact]
        public void Test_two_packets()
        {
            var ms = new MemoryStream(SamplesData.Response1.Concat(SamplesData.Response2).ToArray());
            var resp = MessageParser.ReadPacket(ms).Result;
            resp.Success.Should().BeTrue();
            resp.ResultType.Should().Be(PacketResultType.Success);
            resp.Data.Should().Equal(SamplesData.Response1);
            ms.Position.Should().Be(SamplesData.Response1.Length);
            
            resp = MessageParser.ReadPacket(ms).Result;
            resp.Success.Should().BeTrue();
            resp.ResultType.Should().Be(PacketResultType.Success);
            resp.Data.Should().Equal(SamplesData.Response2);
            
            ms.Position.Should().Be(SamplesData.Response1.Length + SamplesData.Response2.Length);
        }
    }
}