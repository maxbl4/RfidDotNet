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
        public void Should_read_known_tags()
        {
            using (var r = new SerialReader(TestSettings.Instance.PortName))
            {
                r.SetRFPower(26).Wait();
                r.SetInventoryScanInterval(TimeSpan.FromSeconds(10)).Wait();
                var tags = new List<Tag>();
                Timing.StartWait(() =>
                {
                    tags.AddRange(r.TagInventory().Result.Tags);
                    return tags.Select(x => x.TagId).Distinct().Count() >= 2;
                }).Result.ShouldBeTrue();
                tags.Select(x => x.TagId)
                    .Intersect(TestSettings.Instance.GetKnownTagIds)
                    .Count()
                    .ShouldBeGreaterThanOrEqualTo(1,
                        $"Should find at least one tag from known tags list. " +
                        $"Actually found: {string.Join(", ", tags.Select(x => x.TagId))}");
                tags[0].Rssi.ShouldBeGreaterThan(0);
                tags[0].ReadCount.ShouldBe(1);
                tags[0].LastSeenTime.ShouldBeGreaterThan(DateTimeOffset.Now.Date);
                tags[0].DiscoveryTime.ShouldBeGreaterThan(DateTimeOffset.Now.Date);
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