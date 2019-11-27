using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using Microsoft.Extensions.Configuration;

namespace maxbl4.RfidDotNet.AlienTech.Simulator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var options = ReadOptions(args);
            Console.WriteLine($"Starting Alien reader protocol simulator on {options.ListenOn}");
            var readerEndpoint = IPEndPoint.Parse(options.ListenOn);
            var simulator = new SimulatorListener(readerEndpoint);
            var tagListHandler = new TagListHandler(options);
            simulator.TagListHandler = tagListHandler.Handle;
            var tags = options.VisibleTags
                .Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new Tag{TagId = x, DiscoveryTime = DateTime.UtcNow, LastSeenTime = DateTime.UtcNow, ReadCount = 1})
                .ToArray();
            if (options.RandomTags)
                Console.WriteLine($"Will return random tags");
            else
                Console.WriteLine($"Found {tags.Length} tags in options");
            tagListHandler.ReturnContinuos(tags);
            Console.WriteLine($"Waiting for connections on {simulator.ListenEndpoint}, press CTRL+C to stop");
            Console.CancelKeyPress += (sender, args) => simulator.Dispose();
            await simulator.ListenTask;
        }

        static SimulatorOptions ReadOptions(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args ?? new string[0])
                .Build();
            return config.Get<SimulatorOptions>();
        }
    }
}