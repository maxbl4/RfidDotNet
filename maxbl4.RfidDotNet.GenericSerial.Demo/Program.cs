using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.Ext;
using maxbl4.RfidDotNet.GenericSerial.Model;
using RJCP.IO.Ports;

namespace maxbl4.RfidDotNet.GenericSerial.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(@"Generic serial reader test app. Supply connection string in either format:
serial://COM4
serial:///dev/ttyS2
tcp://host:123");
                Console.WriteLine("Available serial ports:");
                var names = SerialPortStream.GetPortNames();
                foreach (var name in names)
                {
                    Console.WriteLine(name);
                }
                return;
            }

            var connectionString = ConnectionString.Parse(args[0]);
            using (var r = new SerialReader(connectionString.Connect()))
            {
                if (args.Length > 1)
                    r.SetRFPower(byte.Parse(args[1])).Wait();
                
                Console.WriteLine("Serial number: {0}", r.GetSerialNumber().Result);
                var info = r.GetReaderInfo().Result;
                Console.WriteLine("Model: {0}", info.Model);
                Console.WriteLine("FirmwareVersion: {0}", info.FirmwareVersion);
                Console.WriteLine("AntennaConfiguration: {0}", info.AntennaConfiguration);
                Console.WriteLine("SupportedProtocols: {0}", info.SupportedProtocols);
                Console.WriteLine("RFPower: {0}", info.RFPower);
                Console.WriteLine("InventoryScanInterval: {0}", info.InventoryScanInterval);


                if (args.Length < 3)
                {
                    Console.WriteLine("Press enter to start inventory cycle");
                    Console.ReadLine();
                }

                Console.WriteLine("Performing inventory. Ctrl+C to stop");
                
                var tags = new Subject<TagInventoryResult>();
                tags.Buffer(TimeSpan.FromMilliseconds(1000))
                    .Where(x => x.Count > 0)
                    .Subscribe(buf =>
                    {
                        Console.Clear();
                        Console.WriteLine($"Connected to: {connectionString}");
                        var bufferedTags = buf.SelectMany(x => x.Tags).ToList();
                        var inventoryQueryAverageTimeMs = (int)buf.Average(x => x.Elapsed.TotalMilliseconds);
                        var inventoryQueryMaxTimeMs = (int)buf.Max(x => x.Elapsed.TotalMilliseconds);
                        var inventoryQueryMinTimeMs = (int)buf.Min(x => x.Elapsed.TotalMilliseconds);
                        var histogram = bufferedTags.GroupBy(x => x.TagId).Select(x => new {TagId = x.Key, Count = x.Count()})
                            .OrderBy(x => x.TagId)
                            .ToList();
                        Console.WriteLine($"TagIds={histogram.Count}, RPS={bufferedTags.Count}, InventoryPS={buf.Count}");
                        Console.WriteLine($"Inventory duration: Min={inventoryQueryMinTimeMs}, Avg={inventoryQueryAverageTimeMs}, Max={inventoryQueryMaxTimeMs}");
                        foreach (var h in histogram)
                        {
                            Console.WriteLine($"{h.TagId} {h.Count}");
                        }
                    });
                Task.Run(async () =>
                {
                    try
                    {
                        while (true)
                        {
                            var res = await r.TagInventory();
                            tags.OnNext(res);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });

                Console.ReadLine();
            }
        }
    }
}