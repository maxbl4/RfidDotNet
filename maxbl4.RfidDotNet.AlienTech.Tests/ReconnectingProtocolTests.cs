using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AwesomeAssertions;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.Exceptions;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests
{
    /// <summary>
    /// These cover connect and shutdown, not steady state reading. A gateway that
    /// auto restarts on errors has to be able to tell a real reader fault from the
    /// normal noise of a connection going away, so "no unexpected exception" is a
    /// behaviour worth asserting.
    /// </summary>
    public class ReconnectingProtocolTests
    {
        private static IPEndPoint FreeLocalEndpoint() => IPEndPoint.Parse("127.0.0.1:0");

        [Fact]
        public void Should_report_connection_failure_without_null_reference()
        {
            // Nothing is listening on this port. Connect() used to fall through to
            // onConnected with a null proto and raise NullReferenceException, which
            // hid the actual connection error.
            var errors = new List<Exception>();
            var cs = ConnectionString.Parse("protocol=Alien;Network=127.0.0.1:1;RfPower=200");
            using var r = new ReconnectingAlienReaderProtocol(cs);
            r.ReconnectTimeout = 60000;
            r.Errors.Subscribe(errors.Add);

            r.Start().Wait(30000).Should().BeTrue();

            r.IsConnected.Should().BeFalse();
            errors.Should().NotBeEmpty();
            errors.Should().NotContain(x => x is NullReferenceException);
        }

        [Fact]
        public async Task Should_connect_and_stop_without_unexpected_exceptions()
        {
            using var simulator = new SimulatorListener(FreeLocalEndpoint());
            var port = simulator.ListenEndpoint.Port;
            var errors = new List<Exception>();
            var connected = new List<bool>();

            var cs = ConnectionString.Parse(
                $"protocol=Alien;Network=127.0.0.1:{port};RfPower=200;InventoryDuration=500");
            var r = new ReconnectingAlienReaderProtocol(cs);
            r.ReconnectTimeout = 60000;
            r.Errors.Subscribe(errors.Add);
            r.Connected.Subscribe(connected.Add);

            await r.Start();
            r.IsConnected.Should().BeTrue();
            connected.Should().Contain(true);

            r.Dispose();
            await Task.Delay(1000);
            // A clean shutdown reports nothing at all; if a request was still in flight
            // it must surface as the domain error, never as ObjectDisposedException on
            // an internal semaphore or a NullReferenceException on a stream that is gone.
            errors.Should().NotContain(x => !(x is ConnectionLostException),
                "shutdown must not look like a reader fault");
        }

        [Fact]
        public async Task Dispose_should_not_schedule_another_reconnect()
        {
            using var simulator = new SimulatorListener(FreeLocalEndpoint());
            var port = simulator.ListenEndpoint.Port;
            var connections = 0;
            simulator.OnClientAccepted = _ => connections++;

            var cs = ConnectionString.Parse($"protocol=Alien;Network=127.0.0.1:{port};RfPower=200");
            var r = new ReconnectingAlienReaderProtocol(cs);
            r.ReconnectTimeout = 200;
            await r.Start();
            connections.Should().Be(1);

            // Disposing the protocol fires Disconnected, which used to schedule a
            // reconnect to a reader nobody is listening to any more.
            r.Dispose();
            await Task.Delay(1500);
            connections.Should().Be(1);
        }

        [Fact]
        public void Dispose_should_be_idempotent()
        {
            var cs = ConnectionString.Parse("protocol=Alien;Network=127.0.0.1:1;RfPower=200");
            var r = new ReconnectingAlienReaderProtocol(cs);
            r.Dispose();
            r.Dispose();
        }
    }
}
