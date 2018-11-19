using System;
using System.Collections.Generic;
using System.Linq;
using maxbl4.RfidDotNet.Ext;
using RJCP.IO.Ports;

namespace maxbl4.RfidDotNet.GenericSerial.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Generic serial reader test app. Supply serial port name as argument.");
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

            var portName = args[0];
            using (var r = new SerialReader(portName))
            {
                Console.WriteLine("Serial number: {0}", r.GetSerialNumber().Result);
                var info = r.GetReaderInfo().Result;
                Console.WriteLine("Model: {0}", info.Model);
                Console.WriteLine("FirmwareVersion: {0}", info.FirmwareVersion);
                Console.WriteLine("AntennaConfiguration: {0}", info.AntennaConfiguration);
                Console.WriteLine("SupportedProtocols: {0}", info.SupportedProtocols);
                Console.WriteLine("RFPower: {0}", info.RFPower);
                Console.WriteLine("InventoryScanInterval: {0}", info.InventoryScanInterval);
                
                Console.WriteLine("Performing inventory. Ctrl+C to stop");
                var tags = new Dictionary<string, int>();
                while (true)
                {
                    try
                    {
                        var result = r.TagInventory().Result;
                        foreach (var tag in result.Tags)
                        {
                            tags.AddOrUpdate(tag.TagId, 1, (k, v) => v + 1);
                        }

                        Console.WriteLine($"#################################################");
                        foreach (var pair in tags.OrderBy(x => x.Key))
                        {
                            Console.WriteLine($"{pair.Key} {pair.Value}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }
    }
}