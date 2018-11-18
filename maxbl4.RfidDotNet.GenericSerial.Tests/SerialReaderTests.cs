using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using maxbl4.RfidDotNet.GenericSerial.Model;
using maxbl4.RfidDotNet.GenericSerial.Packets;
using maxbl4.RfidDotNet.Infrastructure;
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
                info.RFPower.ShouldBeInRange((byte)0, (byte)33);
                info.InventoryScanInterval.ShouldBeInRange(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(25500));
                info.AntennaConfiguration.ShouldBe(AntennaConfiguration.Antenna1);
                info.BuzzerEnabled.ShouldBe(false);
                info.AntennaCheck.ShouldBe(false);
            }
        }
        
        [Fact]
        public void Should_set_inventory_scan_interval()
        {
            using (var r = new SerialReader(TestSettings.Instance.PortName))
            {
                r.SetInventoryScanInterval(TimeSpan.FromMilliseconds(1000)).Wait();
                r.GetReaderInfo().Result.InventoryScanInterval.ShouldBe(TimeSpan.FromMilliseconds(1000));
                r.SetInventoryScanInterval(TimeSpan.FromMilliseconds(300)).Wait();
                r.GetReaderInfo().Result.InventoryScanInterval.ShouldBe(TimeSpan.FromMilliseconds(300));
            }
        }
        
        [Fact]
        public void Should_set_rf_power()
        {
            using (var r = new SerialReader(TestSettings.Instance.PortName))
            {
                r.SetRFPower(20).Wait();
                r.GetReaderInfo().Result.RFPower.ShouldBe((byte)20);
                r.SetRFPower(0).Wait();
                r.GetReaderInfo().Result.RFPower.ShouldBe((byte)0);
                r.SetRFPower(26).Wait();
                r.GetReaderInfo().Result.RFPower.ShouldBe((byte)26);
            }
        }
        
        [Fact]
        public void Should_get_and_set_epc_length()
        {
            using (var r = new SerialReader(TestSettings.Instance.PortName))
            {
                r.SetEpcLengthForBufferOperations(EpcLength.UpTo496Bits).Wait();
                r.GetEpcLengthForBufferOperations().Result.ShouldBe(EpcLength.UpTo496Bits);
                r.SetEpcLengthForBufferOperations(EpcLength.UpTo128Bits).Wait();
                r.GetEpcLengthForBufferOperations().Result.ShouldBe(EpcLength.UpTo128Bits);
            }
        }
        
        [Fact]
        public void Should_get_number_of_tags_in_buffer()
        {
            using (var r = new SerialReader(TestSettings.Instance.PortName))
            {
                r.GetNumberOfTagsInBuffer().Wait();
            }
        }
        
        [Fact]
        public void Should_run_tag_inventory_with_default_params()
        {
            using (var r = new SerialReader(TestSettings.Instance.PortName))
            {
                r.TagInventory().Wait();
            }
        }
        
        
        [Fact]
        public void Should_run_tag_inventory_with_optional_params()
        {
            using (var r = new SerialReader(TestSettings.Instance.PortName))
            {
                r.TagInventory(new TagInventoryParams
                {
                    QValue = 10,
                    Session = SessionValue.S1,
                    OptionalParams = new TagInventoryOptionalParams(TimeSpan.FromMilliseconds(1000))
                }).Wait();
            }
        }
        
        [Fact]
        public void Should_run_inventory_with_buffer()
        {
            using (var r = new SerialReader(TestSettings.Instance.PortName))
            {
                r.TagInventoryWithMemoryBuffer().Wait();
            }
        }
        
        [Fact]
        public void Should_run_inventory_with_buffer_and_get_response()
        {
            using (var r = new SerialReader(TestSettings.Instance.PortName))
            {
                var buffer = 0;
                var lastInventoryAgg = 0;
                Timing.StartWait(() =>
                {
                    var res = r.TagInventoryWithMemoryBuffer().Result;
                    buffer = res.TagsInBuffer;
                    lastInventoryAgg += res.TagsInLastInventory;
                    return lastInventoryAgg > 50;
                }).Result.ShouldBeTrue("Failed to read 50 tags");
                lastInventoryAgg.ShouldBeGreaterThan(50);
                buffer.ShouldBeInRange(1, 100);
                
                r.GetNumberOfTagsInBuffer().Result.ShouldBe(buffer);
            }
        }
        
        //[Fact]
        [Trait("Hardware", "true")]
        [Trait("MultiAntenna", "true")]
        public void Should_set_antenna_configuration()
        {
            using (var r = new SerialReader(TestSettings.Instance.PortName))
            {
                r.SetAntennaConfiguration(AntennaConfiguration.Antenna1).Wait();
                r.GetReaderInfo().Result.AntennaConfiguration.ShouldBe(AntennaConfiguration.Antenna1);
                r.SetAntennaConfiguration(AntennaConfiguration.Antenna1|AntennaConfiguration.Antenna2).Wait();
                r.GetReaderInfo().Result.AntennaConfiguration.ShouldBe(AntennaConfiguration.Antenna1|AntennaConfiguration.Antenna2);
            }
        }
        
        //[Fact]
        [Trait("Hardware", "true")]
        [Trait("MultiAntenna", "true")]
        public void Should_set_antenna_check()
        {
            using (var r = new SerialReader(TestSettings.Instance.PortName))
            {
                r.SetAntennaCheck(true).Wait();
                r.GetReaderInfo().Result.AntennaCheck.ShouldBeTrue();
                r.SetAntennaCheck(false).Wait();
                r.GetReaderInfo().Result.AntennaCheck.ShouldBeFalse();
            }
        }
    }
}