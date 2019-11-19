using maxbl4.RfidDotNet.AlienTech.Extensions;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests
{
    public class ExtTests
    {
        [Fact]
        public void Should_convert_antenna_config_to_alien_sequence()
        {
            ((AntennaConfiguration?)null).ToAlienAntennaSequence().ShouldBe(null);
            AntennaConfiguration.Nothing.ToAlienAntennaSequence().ShouldBe("");
            AntennaConfiguration.Antenna1.ToAlienAntennaSequence().ShouldBe("0");
            AntennaConfiguration.Antenna2.ToAlienAntennaSequence().ShouldBe("1");
            AntennaConfiguration.Antenna3.ToAlienAntennaSequence().ShouldBe("2");
            AntennaConfiguration.Antenna4.ToAlienAntennaSequence().ShouldBe("3");
            (AntennaConfiguration.Antenna1|AntennaConfiguration.Antenna2)
                .ToAlienAntennaSequence().ShouldBe("0 1");
            (AntennaConfiguration.Antenna1|AntennaConfiguration.Antenna2
              |AntennaConfiguration.Antenna3|AntennaConfiguration.Antenna4)
                .ToAlienAntennaSequence().ShouldBe("0 1 2 3");
        }

        [Fact]
        public void Should_convert_alien_sequence_to_antenna_config()
        {
            "".ParseAlienAntennaSequence().ShouldBe(AntennaConfiguration.Nothing);
            "0".ParseAlienAntennaSequence().ShouldBe(AntennaConfiguration.Antenna1);
            "1".ParseAlienAntennaSequence().ShouldBe(AntennaConfiguration.Antenna2);
            "2".ParseAlienAntennaSequence().ShouldBe(AntennaConfiguration.Antenna3);
            "3".ParseAlienAntennaSequence().ShouldBe(AntennaConfiguration.Antenna4);
            
            "0 1".ParseAlienAntennaSequence().ShouldBe(AntennaConfiguration.Antenna1|AntennaConfiguration.Antenna2);
            "1 0".ParseAlienAntennaSequence().ShouldBe(AntennaConfiguration.Antenna1|AntennaConfiguration.Antenna2);
            
            "0 1 3 2".ParseAlienAntennaSequence().ShouldBe(AntennaConfiguration.Antenna1
                   |AntennaConfiguration.Antenna2
                   |AntennaConfiguration.Antenna3
                   |AntennaConfiguration.Antenna4);
        }
    }
}