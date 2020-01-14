using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.AlienTech.TagStream;

namespace maxbl4.RfidDotNet.AlienTech.Simulator
{
    public class TagListHandler
    {
        private readonly SimulatorOptions simulatorOptions;
        readonly object sync = new object();
        private Tag[] returnContinuos;
        DateTime lastInfo = DateTime.UtcNow;
        private int requestCounter = 0;
        private readonly RandomTagGenerator randomTagGenerator = new RandomTagGenerator();
        private readonly Random random = new Random();
            
        public TagListHandler(SimulatorOptions simulatorOptions)
        {
            this.simulatorOptions = simulatorOptions;
        }

        public string Handle()
        {
            lock (sync)
            {
                requestCounter++;
                var interval = (DateTime.UtcNow - lastInfo);
                if (interval > TimeSpan.FromSeconds(1))
                {
                    Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} served {requestCounter} requests for last {interval}");
                    requestCounter = 0;
                    lastInfo = DateTime.UtcNow;
                }
                
                Thread.Sleep(simulatorOptions.ReadLatencyMs);
                
                var tags = new List<Tag>();

                if (simulatorOptions.RandomTags)
                    tags.AddRange(randomTagGenerator.Next());

                if (returnContinuos.Length > 0)
                {
                    if (simulatorOptions.KnownTagsPercent > 99)
                    {
                        tags.AddRange(returnContinuos.Select(x =>
                        {
                            x.LastSeenTime = DateTime.UtcNow;
                            return x;
                        }));
                    }else if (simulatorOptions.KnownTagsPercent > 0)
                    {
                        var take = (int)Math.Round(random.NextDouble() * returnContinuos.Length * simulatorOptions.KnownTagsPercent /
                                     100d);
                        var samples = returnContinuos.ToList();
                        for (int i = 0; i < take; i++)
                        {
                            var index = random.Next(samples.Count);
                            var tag = samples[index];
                            samples.RemoveAt(index);
                            tag.LastSeenTime = DateTime.UtcNow;
                            tags.Add(tag);
                        }
                    }
                }
                
                if (tags.Count == 0)
                    return ProtocolMessages.NoTags;
                return string.Join("\r\n", tags.Select(x => x.ToCustomFormatString()));
            }
        }

        

        public void ReturnContinuos(params Tag[] tags)
        {
            lock (sync)
            {
                returnContinuos = tags;
            }
        }
    }
}