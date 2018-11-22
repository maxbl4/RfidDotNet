using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.Ext;
using RJCP.IO.Ports;

namespace maxbl4.RfidDotNet.GenericSerial.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"Generic serial reader test app. Supply connection string in either format:
serial://COM4
serial:///dev/ttyS2
tcp://host:123");
            if (args.Length == 0)
            {
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
                Console.WriteLine("Serial number: {0}", r.GetSerialNumber().Result);
                var info = r.GetReaderInfo().Result;
                Console.WriteLine("Model: {0}", info.Model);
                Console.WriteLine("FirmwareVersion: {0}", info.FirmwareVersion);
                Console.WriteLine("AntennaConfiguration: {0}", info.AntennaConfiguration);
                Console.WriteLine("SupportedProtocols: {0}", info.SupportedProtocols);
                Console.WriteLine("RFPower: {0}", info.RFPower);
                Console.WriteLine("InventoryScanInterval: {0}", info.InventoryScanInterval);

                r.SetRFPower(30).Wait();
                
                Console.WriteLine("Performing inventory. Ctrl+C to stop");
                var tags = new Subject<List<Tag>>();
                tags.SelectMany(x => x).Buffer(TimeSpan.FromMilliseconds(1000))
                    .Where(x => x.Count > 0)
                    .Subscribe(buf =>
                    {
                        Console.Clear();
                        Console.WriteLine("Model: {0}", info.Model);
                        Console.WriteLine("FirmwareVersion: {0}", info.FirmwareVersion);
                        Console.WriteLine("AntennaConfiguration: {0}", info.AntennaConfiguration);
                        Console.WriteLine("SupportedProtocols: {0}", info.SupportedProtocols);
                        Console.WriteLine("RFPower: {0}", info.RFPower);
                        var histogram = buf.GroupBy(x => x.TagId).Select(x => new {TagId = x.Key, Count = x.Count()})
                            .OrderBy(x => x.TagId)
                            .ToList();
                        Console.WriteLine($"TagIds={histogram.Count}, RPS={buf.Count}");
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
                            tags.OnNext(res.Tags);
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