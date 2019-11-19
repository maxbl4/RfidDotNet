using System;
using System.Linq;
using System.Net;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.AlienTech.TagStream;
using Microsoft.Extensions.Configuration;

namespace maxbl4.RfidDotNet.AlienTech.Simulator
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = ReadOptions(args);
            Console.WriteLine($"Starting Alien reader protocol simulator on {options.ListenOn}");
            var readerEndpoint = IPEndPoint.Parse(options.ListenOn);
            var simulator = new SimulatorListener(readerEndpoint);
            var tagListHandler = new TagListHandler();
            simulator.TagListHandler = tagListHandler.Handle;
            var tags = options.VisibleTags
                .Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new Tag{TagId = x, DiscoveryTime = DateTimeOffset.UtcNow, LastSeenTime = DateTimeOffset.UtcNow, ReadCount = 1})
                .ToArray();
            Console.WriteLine($"Found {tags.Length} tags in options");
            tagListHandler.ReturnContinuos(tags);
            Console.WriteLine($"Waiting for connections on {simulator.ListenEndpoint}, press ENTER to stop");
            Console.ReadLine();
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