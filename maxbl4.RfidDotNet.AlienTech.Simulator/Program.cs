using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace maxbl4.RfidDotNet.AlienTech.Simulator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var options = ReadOptions(args);
            var logger = Log.ForContext<Program>();
            logger.Information($"Starting Alien reader protocol simulator on {options.ListenOn}");
            var readerEndpoint = IPEndPoint.Parse(options.ListenOn);
            var simulator = new SimulatorListener(readerEndpoint);
            var tagListHandler = new TagListHandler(options);
            simulator.TagListHandler = tagListHandler.Handle;
            var tags = options.KnownTags
                .Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new Tag{TagId = x, DiscoveryTime = DateTime.UtcNow, LastSeenTime = DateTime.UtcNow, ReadCount = 1})
                .ToArray();
            if (options.RandomTags)
                logger.Information($"Will return random tags");
            else
                logger.Information($"Found {tags.Length} tags in options");
            tagListHandler.ReturnContinuos(tags);
            logger.Information($"Waiting for connections on {simulator.ListenEndpoint}, press CTRL+C to stop");
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
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();
            return config.GetSection(nameof(SimulatorOptions)).Get<SimulatorOptions>();
        }
    }
}