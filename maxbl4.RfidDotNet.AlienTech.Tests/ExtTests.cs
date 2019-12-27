using FluentAssertions;
using maxbl4.RfidDotNet.AlienTech.Extensions;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests
{
    public class ExtTests
    {
        [Fact]
        public void Should_convert_antenna_config_to_alien_sequence()
        {
            ((AntennaConfiguration?)null).ToAlienAntennaSequence().Should().Be(null);
            AntennaConfiguration.Nothing.ToAlienAntennaSequence().Should().Be("");
            AntennaConfiguration.Antenna1.ToAlienAntennaSequence().Should().Be("0");
            AntennaConfiguration.Antenna2.ToAlienAntennaSequence().Should().Be("1");
            AntennaConfiguration.Antenna3.ToAlienAntennaSequence().Should().Be("2");
            AntennaConfiguration.Antenna4.ToAlienAntennaSequence().Should().Be("3");
            (AntennaConfiguration.Antenna1|AntennaConfiguration.Antenna2)
                .ToAlienAntennaSequence().Should().Be("0 1");
            (AntennaConfiguration.Antenna1|AntennaConfiguration.Antenna2
              |AntennaConfiguration.Antenna3|AntennaConfiguration.Antenna4)
                .ToAlienAntennaSequence().Should().Be("0 1 2 3");
        }

        [Fact]
        public void Should_convert_alien_sequence_to_antenna_config()
        {
            "".ParseAlienAntennaSequence().Should().Be(AntennaConfiguration.Nothing);
            "0".ParseAlienAntennaSequence().Should().Be(AntennaConfiguration.Antenna1);
            "1".ParseAlienAntennaSequence().Should().Be(AntennaConfiguration.Antenna2);
            "2".ParseAlienAntennaSequence().Should().Be(AntennaConfiguration.Antenna3);
            "3".ParseAlienAntennaSequence().Should().Be(AntennaConfiguration.Antenna4);
            
            "0 1".ParseAlienAntennaSequence().Should().Be(AntennaConfiguration.Antenna1|AntennaConfiguration.Antenna2);
            "1 0".ParseAlienAntennaSequence().Should().Be(AntennaConfiguration.Antenna1|AntennaConfiguration.Antenna2);
            
            "0 1 3 2".ParseAlienAntennaSequence().Should().Be(AntennaConfiguration.Antenna1
                   |AntennaConfiguration.Antenna2
                   |AntennaConfiguration.Antenna3
                   |AntennaConfiguration.Antenna4);
        }
    }
}