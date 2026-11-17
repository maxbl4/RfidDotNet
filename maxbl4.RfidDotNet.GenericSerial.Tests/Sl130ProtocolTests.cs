using System;
using System.IO;
using AwesomeAssertions;
using maxbl4.RfidDotNet.GenericSerial.Sl130;
using Xunit;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    public class Sl130ProtocolTests
    {
        private static readonly byte[] WorkModeResponse =
        {
            0x11, 0x00, 0x36, 0x00, 0x00, 0x01, 0x0A, 0x0F, 0x01,
            0x08, 0x05, 0x09, 0x01, 0x00, 0x08, 0x05, 0x73, 0x6F,
        };

        private static readonly byte[] ReaderInfoResponse =
        {
            0x0D, 0x00, 0x21, 0x00, 0x03, 0x3F, 0x09, 0x03,
            0x4E, 0x00, 0x1E, 0x0A, 0x83, 0xF0,
        };

        private static readonly byte[] TagFrame =
        {
            0x11, 0x00, 0xEE, 0x00, 0xE2, 0x00, 0x60, 0x08, 0x13,
            0x14, 0x02, 0x34, 0x12, 0x60, 0x95, 0x46, 0xAC, 0xE9,
        };

        [Fact]
        public void Should_build_commands_with_protocol_crc()
        {
            Sl130Protocol.BuildCommand(Sl130Protocol.GetWorkMode)
                .Should().Equal(0x04, 0x00, 0x36, 0xE7, 0x0E);
            Sl130Protocol.BuildCommand(Sl130Protocol.SetWorkMode,
                    0x01, 0x0E, 0x05, 0x09, 0x01, 0x00)
                .Should().Equal(0x0A, 0x00, 0x35, 0x01, 0x0E, 0x05, 0x09, 0x01, 0x00, 0xC3, 0x02);
        }

        [Fact]
        public void Should_parse_live_work_mode_and_make_it_suitable_for_racing()
        {
            var current = Sl130Protocol.ParseWorkMode(WorkModeResponse);
            current.ReadMode.Should().Be(Sl130Protocol.ScanMode);
            current.ModeState.Should().Be(0x08);
            current.MemInven.Should().Be(0x05);

            var required = Sl130Protocol.ForContinuousInventory(current);
            required.ReadMode.Should().Be(Sl130Protocol.ScanMode);
            required.ModeState.Should().Be(0x0E);
            required.MemInven.Should().Be(Sl130Protocol.InventoryMultiple);
            required.FirstAdr.Should().Be(current.FirstAdr);
            required.WordNum.Should().Be(current.WordNum);
            required.TagTime.Should().Be(current.TagTime);
            Sl130Protocol.SerializeWorkMode(required).Should().Equal(0x01, 0x0E, 0x04, 0x09, 0x01, 0x00);
            Sl130Protocol.ForContinuousInventory(required).Should().Be(required);
        }

        [Fact]
        public void Should_parse_short_reader_information_response()
        {
            Sl130Protocol.ParseReaderInfo(ReaderInfoResponse).RfPower.Should().Be(30);
        }

        [Fact]
        public void Should_create_tag_from_live_report()
        {
            var now = DateTime.UtcNow;
            Sl130Protocol.TryCreateTag(TagFrame, now, out var tag).Should().BeTrue();
            tag.TagId.Should().Be("E20060081314023412609546");
            tag.Antenna.Should().Be(0);
            tag.ReadCount.Should().Be(1);
            tag.DiscoveryTime.Should().Be(now);
            tag.LastSeenTime.Should().Be(now);
        }

        [Fact]
        public void Should_reject_bad_crc_and_error_status()
        {
            var badCrc = (byte[])TagFrame.Clone();
            badCrc[^1] ^= 0xFF;
            Sl130Protocol.TryCreateTag(badCrc, DateTime.UtcNow, out _).Should().BeFalse();

            var action = () => Sl130Protocol.ValidateResponse(
                new byte[] { 0x05, 0x00, 0x35, 0xFD, 0x06, 0x89 }, Sl130Protocol.SetWorkMode);
            action.Should().Throw<InvalidDataException>().WithMessage("*0xFD*");
        }

        [Fact]
        public void Should_reject_response_with_unexpected_length_or_command()
        {
            var action = () => Sl130Protocol.ParseReaderInfo(WorkModeResponse);
            action.Should().Throw<InvalidDataException>().WithMessage("*expected 14*");

            var response = (byte[])ReaderInfoResponse.Clone();
            response[2] = 0x36;
            action = () => Sl130Protocol.ParseReaderInfo(response);
            action.Should().Throw<InvalidDataException>();
        }

        [Fact]
        public void Should_enter_answer_mode_for_pull_inventory()
        {
            var current = Sl130Protocol.ParseWorkMode(WorkModeResponse);
            var required = Sl130Protocol.ForPullInventory(current);
            required.ReadMode.Should().Be(Sl130Protocol.AnswerMode);
            required.ModeState.Should().Be(current.ModeState);
            required.MemInven.Should().Be(current.MemInven);
        }

        [Fact]
        public void Should_build_pull_inventory_without_parameters()
        {
            Sl130Protocol.BuildCommand(Sl130Protocol.TagInventory)
                .Should().Equal(0x04, 0x00, 0x01, 0xDB, 0x4B);
        }

        [Fact]
        public void Should_parse_live_pull_inventory_response()
        {
            byte[] frame =
            {
                0x2D, 0x00, 0x01, 0x01, 0x03, 0x0C, 0x03, 0x07, 0x26, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x92, 0xD5, 0x0C, 0xE2, 0x00, 0x00, 0x17, 0x29,
                0x18, 0x01, 0x57, 0x28, 0x80, 0x02, 0x03, 0x0C, 0x03, 0x07, 0x26, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0xB1, 0x89, 0x59,
            };
            var now = DateTime.UtcNow;
            var tags = Sl130Protocol.ParseInventory(frame, now);
            tags.Count.Should().Be(3);
            tags[0].TagId.Should().Be("0307260000000000000092D5");
            tags[1].TagId.Should().Be("E20000172918015728800203");
            tags[2].TagId.Should().Be("0307260000000000000090B1");
            tags[0].DiscoveryTime.Should().Be(now);
            tags[0].LastSeenTime.Should().Be(now);
            tags[0].Antenna.Should().Be(0);
            tags[0].Rssi.Should().Be(0);
        }

        [Fact]
        public void Should_treat_no_tags_as_completed_pull_inventory()
        {
            Sl130Protocol.ParseInventory(
                    new byte[] { 0x05, 0x00, 0x01, 0xFB, 0xF2, 0x3D }, DateTime.UtcNow)
                .Should().BeEmpty();
        }
    }
}
