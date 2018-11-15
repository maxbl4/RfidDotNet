using System;
using System.Runtime.InteropServices;
using System.Threading;
using maxbl4.RfidDotNet.GenericSerial.Packets;
using RJCP.IO.Ports;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    [Collection("Hardware")]
    public class SerialReaderTests
    {
        [Fact]
        public void Should_get_serial_number()
        {
            using (var r = new SerialReader(TestSettings.ComPort))
            {
                r.GetSerialNumber().ShouldBe(10);
                //r.GetSerialNumber().ShouldBe(17439015); // real serial number
            }
        }
    }
}