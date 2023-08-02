using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.GenericSerial.Model;
using maxbl4.RfidDotNet.Infrastructure;
using Microsoft.Extensions.Configuration;
using PowerArgs;
using Serilog;

namespace maxbl4.RfidDotNet.GenericSerial.Demo
{
    class Program
    {
        static readonly Subject<TagInventoryResultWithProcessingTime> pollingResults = new();
        static readonly Subject<Tag> tagStream = new();
        static readonly Subject<Exception> tagStreamErrors = new();
        static readonly BehaviorSubject<int> temperatureSubject = new(0);
        private static ConnectionString connectionString;
        private static int updateNumber = 0;

        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true)
                .AddJsonFile($"appsettings.Development.json", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args ?? new string[0])
                .Build();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();
            
            if (args.Length == 0)
            {
                ShowUsageAndExit();
            }

            try
            {
                var demoArgs = Args.Parse<DemoArgs>(args);
                connectionString = ConnectionString.Parse(demoArgs.ConnectionString);
                if (!connectionString.IsValid(out var msg))
                {
                    Console.WriteLine($"Connection string is invalid: {msg}");
                    ShowUsageAndExit();
                }

                using var reader = new SerialReader(new SerialConnectionString(connectionString).Connect());
                reader.Errors.Subscribe(e => Console.WriteLine(e.Message));
                reader.ThrowOnIllegalCommandError = false;
                reader.ActivateOnDemandInventoryMode(true).Wait();
                ShowBasicReaderInfo(reader, demoArgs);
                SubscribeToInventoryResults(reader, demoArgs);
                SetInventoryOptions(reader, demoArgs);
                switch (demoArgs.Inventory)
                {
                    case InventoryType.Poll:
                        StartPolling(reader, demoArgs);
                        break;
                    case InventoryType.Realtime:
                        StartStreaming(reader);
                        break;
                }
                Console.ReadLine();
                reader.ActivateOnDemandInventoryMode(true).Wait();
            }
            catch (ArgException ex)
            {
                Console.WriteLine(ex.Message);
                ShowUsageAndExit();
            }
        }

        static void SetInventoryOptions(SerialReader reader, DemoArgs demoArgs)
        {
            var cs = ConnectionString.Parse(demoArgs.ConnectionString);
            reader.SetAntennaCheck(false).Wait();
            reader.SetAntennaConfiguration((GenAntennaConfiguration)cs.AntennaConfiguration).Wait();
            reader.SetDrmEnabled(demoArgs.EnableDrmMode).Wait();
            reader.SetRealTimeInventoryParameters(new RealtimeInventoryParams
            {
                QValue = (byte)cs.QValue,
                Session = (SessionValue)cs.Session,
                TagDebounceTime = TimeSpan.Zero
            }).Wait();
            reader.SetInventoryScanInterval(TimeSpan.FromMilliseconds(cs.InventoryDuration)).Wait();
        }

        static void StartStreaming(SerialReader reader)
        {
            reader.Tags.Subscribe(tagStream);
            reader.Errors.Subscribe(tagStreamErrors);
            reader.ActivateRealtimeInventoryMode().Wait();
        }

        static void StartPolling(SerialReader reader, DemoArgs demoArgs)
        {
            var cs = ConnectionString.Parse(demoArgs.ConnectionString);
            Task.Run(async () =>
            {
                temperatureSubject.OnNext(reader.GetReaderTemperature().Result);
                var processingTime = new Stopwatch();
                var sw = Stopwatch.StartNew();
                while (true)
                {
                    try
                    {
                        processingTime.Stop();
                        var pTime = processingTime.Elapsed;
                        var res = await reader.TagInventory(new TagInventoryParams
                        {
                            QValue = (byte)cs.QValue,
                            Session = (SessionValue)cs.Session
                        });
                        processingTime.Restart();
                        
                        pollingResults.OnNext(new TagInventoryResultWithProcessingTime
                        {
                            Result =res,
                            ProcessingTime = pTime
                        });

                        if (sw.ElapsedMilliseconds > 10000)
                        {
                            var t = reader.GetReaderTemperature().Result;
                            if (t > demoArgs.ThermalLimit)
                            {
                                Console.WriteLine(
                                    $"Reader is overheating, temperature is {t}. To prevent damage, stopping inventory.");
                                Environment.Exit(t);
                            }

                            temperatureSubject.OnNext(t);
                            sw.Restart();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            });
        }

        static void SubscribeToInventoryResults(SerialReader reader, DemoArgs demoArgs)
        {
            SubscribeToPollingResults(demoArgs);
            SubscribeToStreamingResults(demoArgs);
        }

        static void SubscribeToStreamingResults(DemoArgs demoArgs)
        {
            tagStreamErrors.Subscribe(x =>
            {
                Console.WriteLine(x);
                Environment.Exit(0);
            });
            tagStream.Buffer(TimeSpan.FromMilliseconds(demoArgs.StatsSamplingInterval))
                .Where(x => x.Count > 0)
                .Subscribe(buf =>
                {
                    var rpsStats = RpsCounter.Count(buf, demoArgs.StatsSamplingInterval);
                    DisplayInventoryInfo(demoArgs, rpsStats, 0, Triple.Empty, Triple.Empty);
                });
        }

        static void SubscribeToPollingResults(DemoArgs demoArgs)
        {
            pollingResults.Buffer(TimeSpan.FromMilliseconds(demoArgs.StatsSamplingInterval))
                .Where(x => x.Count > 0)
                .Subscribe(buf =>
                {
                    var bufferedTags = buf.SelectMany(x => x.Result.Tags).ToList();
                    var inventoryDuration = new Triple(buf.Select(x => x.Result.Elapsed.TotalMilliseconds));
                    var processingDuration = new Triple(buf.Select(x => x.ProcessingTime.TotalMilliseconds));
                    
                    var rpsStats = RpsCounter.Count(bufferedTags, demoArgs.StatsSamplingInterval);
                    
                    DisplayInventoryInfo(demoArgs, rpsStats, buf.Count*1000/demoArgs.StatsSamplingInterval, inventoryDuration, processingDuration);
                });
        }

        static void DisplayInventoryInfo(DemoArgs demoArgs, RpsStats rpsStats, int inventoryPs, 
            Triple inventoryDuration, Triple processingDuration)
        {
            Console.Clear();
            Console.WriteLine($"Connected to: {connectionString}, {demoArgs.Inventory} mode, update {updateNumber++}");
            Console.WriteLine($"Reader Temp={temperatureSubject.Value}, Limit={demoArgs.ThermalLimit}");
            Console.WriteLine($"TagIds={rpsStats.TagIds}, RPS={rpsStats.RPS}, InventoryPS={inventoryPs}");
            Console.WriteLine($"Inventory duration: {inventoryDuration}");
            Console.WriteLine($"Processing duration: {processingDuration}");
            foreach (var h in rpsStats.Histogram)
            {
                Console.Write("{0,6:F1}", h);
            }
            Console.WriteLine($" Avg={rpsStats.Average:F1}");
            foreach (var h in rpsStats.AggTags
                .Where(x => string.IsNullOrWhiteSpace(demoArgs.TagIdFilter) || Regex.IsMatch(x.TagId, demoArgs.TagIdFilter)))
            {
                Console.WriteLine($"{h.TagId} {h.ReadCount}");
            }
        }

        static void ShowBasicReaderInfo(SerialReader reader, DemoArgs args)
        {
            reader.SetRFPower((byte)ConnectionString.Parse(args.ConnectionString).RFPower).Wait();
            Console.WriteLine("Serial number: {0}", reader.GetSerialNumber().Result);
            var info = reader.GetReaderInfo().Result;
            Console.WriteLine("Model: {0}", info.Model);
            Console.WriteLine("FirmwareVersion: {0}", info.FirmwareVersion);
            Console.WriteLine("AntennaConfiguration: {0}", info.AntennaConfiguration);
            Console.WriteLine("SupportedProtocols: {0}", info.SupportedProtocols);
            Console.WriteLine("RFPower: {0}", info.RFPower);
            Console.WriteLine("InventoryScanInterval: {0}", info.InventoryScanInterval);


            if (args.Confirm)
            {
                Console.WriteLine("Press enter to start inventory cycle");
                Console.ReadLine();
            }

            Console.WriteLine("Performing inventory. Ctrl+C to stop");
        }

        static void ShowUsageAndExit()
        {
            Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<DemoArgs>());
            ListSerialPorts();
            Environment.Exit(0);
        }

        static void ListSerialPorts()
        {
            Console.WriteLine("Available serial ports:");
            var names = SerialPort.GetPortNames();
            foreach (var name in names)
            {
                Console.WriteLine(name);
            }
        }
    }

    class TagInventoryResultWithProcessingTime
    {
        public TagInventoryResult Result { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }
}