using System;
using System.Collections.Generic;
using System.Linq;
using maxbl4.Infrastructure.Extensions.ByteArrayExt;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.AlienTech.TagStream;

namespace maxbl4.RfidDotNet.AlienTech.Simulator
{
    public class RandomTagGenerator
    {
        private readonly Random rnd = new();
        
        public IEnumerable<Tag> Next()
        {
            var count = rnd.Next(0, 10) - 5;
            if (count <= 0)
                yield break;
            for (int i = 0; i < count; i++)
            {
                var t = new Tag
                {
                    Antenna = rnd.Next(0, 4),
                    Rssi = rnd.NextDouble() * 70,
                    DiscoveryTime = DateTime.UtcNow,
                    LastSeenTime = DateTime.UtcNow,
                    ReadCount = 1,
                    TagId = NextTagId()
                };
                for (int j = 0; j < rnd.Next(1, 10); j++)
                {
                    yield return t;
                }
            }
        }

        private string NextTagId()
        {
            if (rnd.NextDouble() > 0.5)
            {
                var buf = new byte[12];
                buf[0] = (byte)rnd.Next(200, 220);
                return buf.ToHexString();
            }
            return rnd.Next(200, 220).ToString();
        }
    }
}