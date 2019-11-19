using System;
using System.Reactive.Linq;
using maxbl4.RfidDotNet.AlienTech.Extensions;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using maxbl4.RfidDotNet.Infrastructure;

namespace maxbl4.RfidDotNet.Demo
{
    class Program
    {
        static readonly UniversalTagStreamFactory factory = new UniversalTagStreamFactory();
        private static ConnectionString connectionString;
        private static string errors = "";
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Supply connection string");
                return;
            }

            connectionString = ConnectionString.Parse(args[0]);
            if (!connectionString.IsValid(out var errors))
            {
                Console.WriteLine(errors);
                return;
            }

            factory.UseSerialProtocol();
            factory.UseAlienProtocol();
            using (var stream = factory.CreateStream(connectionString))
            {
                stream.Errors.Subscribe(e => errors +=  e.Message + "\r\n");
                SubscribeToPollingResults(stream.Tags, 500);
                stream.Start().Wait();
                Console.WriteLine("Polling for tags...");
                Console.ReadLine();
            }
        }
        
        static void SubscribeToPollingResults(IObservable<Tag> tags, int samplingInterval)
        {
            tags.Buffer(TimeSpan.FromMilliseconds(samplingInterval))
                .Where(x => x.Count > 0)
                .Subscribe(buf =>
                {
                    var rpsStats = RpsCounter.Count(buf, samplingInterval);
                    DisplayInventoryInfo(rpsStats);
                });
        }

        static void DisplayInventoryInfo(RpsStats rpsStats)
        {
            Console.Clear();
            Console.WriteLine($"Errors: {errors}");
            Console.WriteLine($"Connected to: {connectionString}");
            Console.WriteLine($"TagIds={rpsStats.TagIds}, RPS={rpsStats.RPS}");
            foreach (var h in rpsStats.Histogram)
            {
                Console.Write("{0,6:F1}", h);
            }
            Console.WriteLine($" Avg={rpsStats.Average:F1}");
            foreach (var h in rpsStats.AggTags)
            {
                Console.WriteLine($"{h.TagId} {h.ReadCount}");
            }
        }
    }
}