using System;
using System.Runtime.InteropServices;
using System.Threading;
using maxbl4.RfidDotNet.GenericSerial.Model;
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
            using (var r = new SerialReader(TestSettings.Instance.PortName))
            {
                for (int i = 0; i < 5; i++)
                {
                    r.GetSerialNumber().Result.ShouldBe((uint)0x17439015);
                }
            }
        }
        
        [Fact]
        public void Should_get_reader_info()
        {
            using (var r = new SerialReader(TestSettings.Instance.PortName))
            {
                var info = r.GetReaderInfo().Result;
                info.FirmwareVersion.ShouldBe(new Version(3,1));
                info.Model.ShouldBe(ReaderModel.CF_RU5202);
                info.SupportedProtocols.ShouldBe(ProtocolType.Gen18000_6C);
                info.RFPower.ShouldBe((byte)26);
                info.InventoryInterval.ShouldBe(TimeSpan.FromMilliseconds(300));
                info.AntennaConfiguration.ShouldBe(AntennaConfiguration.Antenna1);
                info.BuzzerEnabled.ShouldBe(true);
                info.AntennaCheck.ShouldBe(false);
            }
        }
    }
}