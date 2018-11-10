using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Xml;
using maxbl4.RfidDotNet.AlienTech.Ext;
using maxbl4.RfidDotNet.AlienTech.TagStream;
using Serilog;

namespace maxbl4.RfidDotNet.AlienTech
{
    public class AlienReaderDiscovery : IDisposable
    {
        static readonly ILogger Logger = Log.ForContext<AlienReaderDiscovery>();
        private readonly UdpClient client;
        public int HeartbeatInterval { get; set; } = 45;
        readonly Subject<ReaderInfo> discovery = new Subject<ReaderInfo>();
        public IObservable<ReaderInfo> Discovery => discovery;

        private List<ReaderInfo> readers = new List<ReaderInfo>();
        public List<ReaderInfo> Readers 
        {
            get 
            {
                lock (readers)
                {
                    readers = readers
                        .Where(x => (DateTime.Now - x.Time).TotalSeconds < HeartbeatInterval)
                        .ToList();
                    return readers.ToList();
                }
            }
        }

        public AlienReaderDiscovery()
        {
            client = new UdpClient(3988);
            new Task(RecieveLoop, TaskCreationOptions.LongRunning).Start();
        }

        private async void RecieveLoop()
        {
            try
            {
                while (true)
                {
                    var result = await client.ReceiveAsync();
                    lock (readers)
                    {
                        var doc = new XmlDocument();
                        doc.Load(new MemoryStream(result.Buffer));
                        var ri = ReaderInfoParser.FromXmlString(doc);
                        discovery.OnNext(ri);
                        readers.Add(ri);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Warning("Receive loop failed {e}", e);
            }
        }

        public void Dispose()
        {
            client.Client.CloseForce();
            client?.Dispose();
        }
    }
}