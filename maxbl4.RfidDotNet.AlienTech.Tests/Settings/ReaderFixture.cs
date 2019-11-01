using System;
using System.Linq;
using System.Net;
using System.Threading;
using maxbl4.RfidDotNet.Ext;
using maxbl4.RfidDotNet.AlienTech.Ext;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using Microsoft.Extensions.Configuration;
using Serilog;
using Shouldly;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace maxbl4.RfidDotNet.AlienTech.Tests.Settings
{
    public class ReaderFixture : IDisposable
    {
        private static readonly ILogger Logger = Log.ForContext<ReaderFixture>();
        public SimulatorListener Simulator { get; }
        public AlienReaderProtocol Proto { get; set; }
        public string Host { get; private set; }
        public int Port { get; private set; }
        public AlienTestSettings Settings { get; }

        public ReaderFixture()
        {
            Settings = new ConfigurationBuilder()
                .AddJsonFile("alien-test-settings.json", true)
                .AddEnvironmentVariables()
                .Build()
                .Get<AlienTestSettings>();
            if (Settings.UseHardwareReader)
            {
                UseEndpoint(Settings.HardwareReaderAddress);
            }else
            {
                Simulator = new SimulatorListener(UseEndpoint(Settings.ReaderSimAddress));
            }
            
            Proto = new AlienReaderProtocol();
            Proto.ConnectAndLogin(Host, Port, "alien", "password")
                .Wait(AlienReaderProtocol.DefaultConnectTimeout).ShouldBeTrue();
        }

        private IPEndPoint UseEndpoint(string endpointString)
        {
            var ep = IPEndPoint.Parse(endpointString);
            Host = ep.Address.ToString();
            Port = ep.Port;
            return ep;
        }

        public void Dispose()
        {
            Proto.DisposeSafe();
            Simulator.DisposeSafe();
            WaitForPhysicalReaderToComeback();
        }

        private void WaitForPhysicalReaderToComeback()
        {
            if (!Settings.UseHardwareReader) return;
            Logger.Debug("will wait for physical reader to restart");
            var i = 0;
            while (i < 200)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    Logger.Debug("Try {i}", i++);
                    var p = new AlienReaderProtocol();
                    p.ConnectAndLogin(Host, Port, "alien", "password", 1000).Wait();
                    sw.Stop();
                    Logger.Debug("[{Elapsed}]Successfully connected to physical reader", sw.Elapsed);
                    return;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    Logger.Debug("[{Elapsed}]Failed to connect to physical reader: {Message}", sw.Elapsed, ex.Message);
                    if (sw.ElapsedMilliseconds < 1000)
                        Thread.Sleep(1000);
                }
            }
            throw new Exception("Could not connect to reader after 200 retries");
        }
        
        public void SetTagListHandlerForKnownTags()
        {
            if (Settings.UseHardwareReader) return;
            var serializedTagList = string.Join("\r\n", Settings.KnownTagIds.Select(x => x.ToTagString()));
            Simulator.TagListHandler = () => serializedTagList;
        }
    }
}