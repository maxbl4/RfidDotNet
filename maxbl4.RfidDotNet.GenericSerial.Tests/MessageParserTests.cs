using System.IO;
using maxbl4.RfidDotNet.GenericSerial.Buffers;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    public class MessageParserTests
    {
        [Fact]
        public void Test_zero_data()
        {
            var ms = new MemoryStream();
            var resp = new MessageParser().ReadPacket(ms);
            resp.Success.ShouldBeFalse();
            resp.ResultType.ShouldBe(PacketResultType.WrongSize);
        }
        
        [Fact]
        public void Test_response1()
        {
            var ms = new MemoryStream(SamplesData.Response1);
            var resp = new MessageParser().ReadPacket(ms);
            resp.Success.ShouldBeTrue();
            resp.ResultType.ShouldBe(PacketResultType.Success);
            resp.Data.ShouldBe(SamplesData.Response1);
        }
    }
}