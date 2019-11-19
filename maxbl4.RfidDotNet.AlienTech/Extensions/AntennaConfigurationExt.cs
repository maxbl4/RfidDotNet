using System;
using System.Collections.Generic;

namespace maxbl4.RfidDotNet.AlienTech.Extensions
{
    public static class AntennaConfigurationExt
    {
        public static string ToAlienAntennaSequence(this AntennaConfiguration conf)
        {
            var sb = new List<char>(4);
            if ((conf & AntennaConfiguration.Antenna1) != 0) sb.Add('0');
            if ((conf & AntennaConfiguration.Antenna2) != 0) sb.Add('1');
            if ((conf & AntennaConfiguration.Antenna3) != 0) sb.Add('2');
            if ((conf & AntennaConfiguration.Antenna4) != 0) sb.Add('3');
            return string.Join(" ", sb);
        }
        
        public static string ToAlienAntennaSequence(this AntennaConfiguration? conf)
        {
            if (conf == null) return null;
            return ToAlienAntennaSequence(conf.Value);
        }

        public static AntennaConfiguration ParseAlienAntennaSequence(this string seq)
        {
            var parts = seq.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var res = AntennaConfiguration.Nothing;
            foreach (var part in parts)
            {
                switch (part)
                {
                    case "0":
                        res |= AntennaConfiguration.Antenna1;
                        break;
                    case "1":
                        res |= AntennaConfiguration.Antenna2;
                        break;
                    case "2":
                        res |= AntennaConfiguration.Antenna3;
                        break;
                    case "3":
                        res |= AntennaConfiguration.Antenna4;
                        break;
                }
            }
            return res;
        }
    }
}