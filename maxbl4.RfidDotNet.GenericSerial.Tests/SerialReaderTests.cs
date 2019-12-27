using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using maxbl4.Infrastructure;
using maxbl4.RfidDotNet.GenericSerial.DataAdapters;
using maxbl4.RfidDotNet.GenericSerial.Model;
using Xunit;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    [Collection("Hardware")]
    [Trait("Hardware", "True")]
    public class SerialReaderTests
    {
        [Fact]
        public void Should_get_serial_number()
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection()))
            {
                for (int i = 0; i < 5; i++)
                {
                    r.GetSerialNumber().Result.Should().BeOneOf((uint)0x17439015, (uint)406196256);
                }
            }
        }
        
        [Fact]
        public void Should_get_reader_info()
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection()))
            {
                //r.SetAntennaCheck(false).Wait();
                r.SetRFPower(20).Wait();
                var info = r.GetReaderInfo().Result;
                info.FirmwareVersion.Major.Should().Be(3);
                info.FirmwareVersion.Minor.Should().BeInRange(1,18);
                new[] {ReaderModel.CF_RU5202, ReaderModel.UHFReader288MP}.Should().Contain(info.Model);
                new[]
                {
                    ProtocolType.Gen18000_6C,
                    ProtocolType.Gen18000_6B, ProtocolType.Gen18000_6C | ProtocolType.Gen18000_6B
                }.Should().Contain(info.SupportedProtocols);
                info.RFPower.Should().Be((byte)20);
                info.InventoryScanInterval.Should().BeLessOrEqualTo(TimeSpan.FromMilliseconds(25500));
                info.AntennaConfiguration.Should().Be(GenAntennaConfiguration.Antenna1);
                info.AntennaCheck.Should().Be(false);
            }
        }
        
        [Fact]
        public void Should_set_inventory_scan_interval()
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection()))
            {
                r.SetInventoryScanInterval(TimeSpan.FromMilliseconds(1000)).Wait();
                r.GetReaderInfo().Result.InventoryScanInterval.Should().Be(TimeSpan.FromMilliseconds(1000));
                r.SetInventoryScanInterval(TimeSpan.FromMilliseconds(300)).Wait();
                r.GetReaderInfo().Result.InventoryScanInterval.Should().Be(TimeSpan.FromMilliseconds(300));
            }
        }
        
        [Fact]
        public void Should_set_rf_power()
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection()))
            {
                r.SetRFPower(20).Wait();
                r.GetReaderInfo().Result.RFPower.Should().Be((byte)20);
                r.SetRFPower(0).Wait();
                r.GetReaderInfo().Result.RFPower.Should().Be((byte)0);
                r.SetRFPower(26).Wait();
                r.GetReaderInfo().Result.RFPower.Should().Be((byte)26);
            }
        }
        
        [Fact]
        public void Should_get_and_set_epc_length()
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection()))
            {
                r.SetEpcLengthForBufferOperations(EpcLength.UpTo496Bits).Wait();
                r.GetEpcLengthForBufferOperations().Result.Should().Be(EpcLength.UpTo496Bits);
                r.SetEpcLengthForBufferOperations(EpcLength.UpTo128Bits).Wait();
                r.GetEpcLengthForBufferOperations().Result.Should().Be(EpcLength.UpTo128Bits);
            }
        }
        
        [Fact]
        public void Should_get_number_of_tags_in_buffer()
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection()))
            {
                r.GetNumberOfTagsInBuffer().Wait();
            }
        }
        
        [Fact]
        public void Should_run_tag_inventory_with_default_params()
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection()))
            {
                r.TagInventory().Wait();
            }
        }
        
        [Fact]
        public void Should_run_tag_inventory_with_optional_params()
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection()))
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
            using (var r = new SerialReader(TestSettings.Instance.GetConnection()))
            {
                r.TagInventoryWithMemoryBuffer().Wait();
            }
        }
        
        [Fact]
        public void Should_clear_buffer()
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection()))
            {
                r.ClearBuffer().Wait();
            }
        }
        
        [Fact]
        public void Should_get_tags_from_buffer_empty()
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection()))
            {
                r.ClearBuffer().Wait();
                var buffer = r.GetTagsFromBuffer().Result;
                buffer.Tags.Count.Should().Be(0);
            }
        }
        
        [SkippableTheory]
        [InlineData(ConnectionType.Serial)]
        [InlineData(ConnectionType.Network)]
        public void Should_read_known_tags(ConnectionType connectionType)
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection(connectionType)))
            {
                r.SetRFPower(10).Wait();
                r.SetInventoryScanInterval(TimeSpan.FromSeconds(10)).Wait();
                var tags = new List<Tag>();
                new Timing().Expect(() =>
                {
                    tags.AddRange(r.TagInventory().Result.Tags);
                    return tags.Select(x => x.TagId).Distinct().Any();
                });
                tags.Select(x => x.TagId)
                    .Intersect(TestSettings.Instance.GetKnownTagIds)
                    .Count()
                    .Should().BeGreaterOrEqualTo(1,
                        $"Should find at least one tag from known tags list. " +
                        $"Actually found: {string.Join(", ", tags.Select(x => x.TagId))}");
                tags[0].Rssi.Should().BeGreaterThan(0);
                tags[0].ReadCount.Should().Be(1);
                tags[0].LastSeenTime.Should().BeAfter(DateTime.UtcNow.Date);
                tags[0].DiscoveryTime.Should().BeAfter(DateTime.UtcNow.Date);
            }
        }
        
        [SkippableTheory]
        [InlineData(ConnectionType.Serial)]
        [InlineData(ConnectionType.Network)]
        public void Should_run_inventory_with_buffer_and_get_response(ConnectionType connectionType)
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection(connectionType)))
            {
                var totalTagsBuffered = 0;
                var lastInventoryAgg = 0;
                new Timing()
                    .Context("Failed to read 50 tags")
                    .Expect(() =>
                {
                    var res = r.TagInventoryWithMemoryBuffer().Result;
                    totalTagsBuffered = res.TagsInBuffer;
                    lastInventoryAgg += res.TagsInLastInventory;
                    return lastInventoryAgg > 50;
                });
                lastInventoryAgg.Should().BeGreaterThan(50);
                totalTagsBuffered.Should().BeInRange(1, 100);
                
                r.GetNumberOfTagsInBuffer().Result.Should().Be(totalTagsBuffered);
                var tagInBuffer = r.GetTagsFromBuffer().Result;
                tagInBuffer.Tags.Count.Should().Be(totalTagsBuffered);
                tagInBuffer.Tags.Select(x => x.TagId)
                    .Intersect(TestSettings.Instance.GetKnownTagIds)
                    .Count()
                    .Should().BeGreaterOrEqualTo(1,
                        $"Should find at least one tag from known tags list. " +
                        $"Actually found: {string.Join(", ", tagInBuffer.Tags.Select(x => x.TagId))}");
                tagInBuffer.Tags[0].Antenna.Should().Be(0);
                tagInBuffer.Tags[0].Rssi.Should().BeGreaterThan(0);
                tagInBuffer.Tags[0].LastSeenTime.Should().BeAfter(DateTime.UtcNow.Date);
                tagInBuffer.Tags[0].DiscoveryTime.Should().BeAfter(DateTime.UtcNow.Date);
            }
        }
        
        [SkippableTheory]
        [InlineData(ConnectionType.Network)]
        [Trait("MultiAntenna", "true")]
        public void Should_set_antenna_configuration(ConnectionType connectionType)
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection(connectionType)))
            {
                r.SetAntennaConfiguration(GenAntennaConfiguration.Antenna1|GenAntennaConfiguration.Antenna2).Wait();
                r.GetReaderInfo().Result.AntennaConfiguration.Should().Be(GenAntennaConfiguration.Antenna1|GenAntennaConfiguration.Antenna2);
                r.SetAntennaConfiguration(GenAntennaConfiguration.Antenna1).Wait();
                r.GetReaderInfo().Result.AntennaConfiguration.Should().Be(GenAntennaConfiguration.Antenna1);
            }
        }
        
        [SkippableTheory]
        [InlineData(ConnectionType.Network)]
        public void Should_get_reader_temperature(ConnectionType connectionType)
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection(connectionType)))
            {
                //Assume we run tests at home :)
                r.GetReaderTemperature().Result.Should().BeInRange(10, 50);
            }
        }
        
        [SkippableTheory]
        [InlineData(ConnectionType.Network)]
        [Trait("MultiAntenna", "true")]
        public void Should_set_antenna_check(ConnectionType connectionType)
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection(connectionType)))
            {
                r.SetAntennaCheck(true).Wait();
                r.GetReaderInfo().Result.AntennaCheck.Should().BeTrue();
                r.SetAntennaCheck(false).Wait();
                r.GetReaderInfo().Result.AntennaCheck.Should().BeFalse();
            }
        }
        
        [SkippableTheory(Skip = "Require reimplementing")]
        [InlineData(ConnectionType.Network)]
        public void Should_read_tags_in_realtime_mode(ConnectionType connectionType)
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection(connectionType)))
            {
                var errors = new List<Exception>();
                r.Errors.Subscribe(errors.Add);
                var tags = new List<Tag>();
                r.Tags.Subscribe(tags.Add);
                r.ActivateOnDemandInventoryMode().Wait();
                r.SetRealTimeInventoryParameters(new RealtimeInventoryParams
                {
                    TagDebounceTime = TimeSpan.Zero
                }).Wait();
                r.ActivateRealtimeInventoryMode().Wait();
                new Timing().Context("Could not read 50 tags in 10 seconds").Expect(() => tags.Count > 50);
                try
                {
                    r.ActivateOnDemandInventoryMode().Wait();
                }
                catch
                {
                }

                if (errors.Count > 0)
                    throw errors[0];
                
                tags[0].Antenna.Should().Be(0);
                tags[0].Rssi.Should().BeGreaterThan(0);
                tags[0].DiscoveryTime.Should().BeAfter(DateTime.UtcNow.Date);
                tags[0].LastSeenTime.Should().BeAfter(DateTime.UtcNow.Date);

                var aggTags = tags.GroupBy(x => x.TagId)
                    .Select(x => new Tag {TagId = x.Key, ReadCount = x.Count()})
                    .ToList();
                
                aggTags.Select(x => x.TagId)
                    .Intersect(TestSettings.Instance.GetKnownTagIds)
                    .Count()
                    .Should().BeGreaterOrEqualTo(1,
                        $"Should find at least one tag from known tags list. " +
                        $"Actually found: {string.Join(", ", aggTags.Select(x => x.TagId))}");
            }
        }

        [SkippableTheory]
        [InlineData(ConnectionType.Network)]
        public void Should_read_and_write_drm_mode(ConnectionType connectionType)
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection(connectionType)))
            {
                r.SetDrmEnabled(true).Result.Should().Be(DrmMode.On);
                r.GetDrmEnabled().Result.Should().Be(true);
                r.SetDrmEnabled(false).Result.Should().Be(DrmMode.Off);
                r.GetDrmEnabled().Result.Should().Be(false);
            }
        }
        
        [SkippableTheory]
        [InlineData(ConnectionType.Serial)]
        public void Should_change_serial_baud_and_update_connection(ConnectionType connectionType)
        {
            var connection = (SerialPortFactory)TestSettings.Instance.GetConnection(connectionType, 
                BaudRates.Baud57600);
            using (var r = new SerialReader(connection))
            {
                var info = r.GetReaderInfo().Result;
                r.SetSerialBaudRate(BaudRates.Baud115200).Wait();
                connection.BaudRate.Should().Be(115200);
                info = r.GetReaderInfo().Result;
                r.SetSerialBaudRate(BaudRates.Baud57600).Wait();
                connection.BaudRate.Should().Be(57600);
                info = r.GetReaderInfo().Result;
            }
        }
        
        [SkippableTheory]
        [InlineData(ConnectionType.Network)]
        public void Should_not_allow_to_change_baud_on_network_reader(ConnectionType connectionType)
        {
            using (var r = new SerialReader(TestSettings.Instance.GetConnection(connectionType)))
            {
                Assert.ThrowsAsync<InvalidOperationException>(() => r.SetSerialBaudRate(BaudRates.Baud115200)).Wait();
            }
        }
    }
}