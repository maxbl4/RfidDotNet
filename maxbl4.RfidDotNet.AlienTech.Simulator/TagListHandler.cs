using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.AlienTech.TagStream;
using Shouldly;

namespace maxbl4.RfidDotNet.AlienTech.Simulator
{
    public class TagListHandler
    {
        private readonly SimulatorOptions simulatorOptions;
        readonly object sync = new object();
        private Tag[] returnContinuos;
        DateTime lastInfo = DateTime.UtcNow;
        private int requestCounter = 0;

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
                
                if (returnContinuos.Length > 0)
                    return string.Join("\r\n", returnContinuos.Select(x =>
                    {
                        x.LastSeenTime = DateTimeOffset.UtcNow;
                        return x.ToCustomFormatString();
                    }));

                return ProtocolMessages.NoTags;
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