using System;
using System.Diagnostics;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Net;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests
{
    public class AlienReaderProtocolTests : IDisposable
    {
        private AlienReaderProtocol proto;
        private SimulatorListener sim;
        
        public AlienReaderProtocolTests()
        {
            sim = new SimulatorListener();
            proto = new AlienReaderProtocol();
            proto.ConnectAndLogin(sim.Host, sim.Port, "alien", "password").Wait(2000).ShouldBeTrue();
        }
        
        [Fact]
        public void Connect_timeout()
        {
            var sw = Stopwatch.StartNew();
            Assert.ThrowsAny<Exception>(() =>
                new AlienReaderProtocol().ConnectAndLogin("10.0.0.254", sim.Port, "alien", "password").Wait());
            sw.Stop();
            sw.ElapsedMilliseconds.ShouldBeInRange(AlienReaderProtocol.DefaultReceiveTimeout, 
                DuplexProtocol.DefaultConnectTimeout + 1000);
        }
        
        [Fact]
        public void Login()
        {
        }
        
        [Fact]
        public async Task RfModulation()
        {
            (await proto.SendReceive("RFModulation = HS")).ShouldBe("RFModulation = HS");
            (await proto.SendReceive("RFModulation?")).ShouldBe("RFModulation = HS");
        }

        [Fact]
        public void LoginWithWrongPassword()
        {
            proto?.Dispose();
            proto = new AlienReaderProtocol();
            Assert.Throws<AggregateException>(() => proto.ConnectAndLogin(sim.Host, sim.Port, "alien", "password1").Wait())
                .InnerException.ShouldBeOfType<LoginFailedException>();
        }
        
        [Fact]
        public async Task SetupReader()
        {
            await SendReceiveConfirm("TagListMillis = ON");
            await SendReceiveConfirm("RFModulation = HS");
            await SendReceiveConfirm("PersistTime = -1");
            await SendReceiveConfirm("TagListAntennaCombine = OFF");
            await SendReceiveConfirm("AntennaSequence = 0");
            await SendReceiveConfirm("TagListFormat = Custom");
            await SendReceiveConfirm("TagListCustomFormat = %k");
            await SendReceiveConfirm("AcqG2Select = 1");
            await SendReceiveConfirm("AcqG2AntennaCombine = OFF");
            await SendReceiveConfirm("RFAttenuation = 100");
            await SendReceiveConfirm("TagStreamMode = OFF");
            await SendReceiveConfirm("AcqG2Q = 3");
            await SendReceiveConfirm("AcqG2QMax = 12");
            await SendReceiveConfirm("AutoModeReset", ProtocolMessages.AutoModeResetConfirmation);
            await SendReceiveConfirm("Clear", ProtocolMessages.TagListClearConfirmation);
        }
        
        [Fact]
        public async Task AutoModeReset()
        {
            (await proto.SendReceive("AutoModeReset")).ShouldBe(ProtocolMessages.AutoModeResetConfirmation);
        }

        [Fact]
        public async Task Clear()
        {
            (await proto.SendReceive("Clear")).ShouldBe(ProtocolMessages.TagListClearConfirmation);
        }

        async Task SendReceiveConfirm(string command, string customValidation = null)
        {
            (await proto.SendReceive(command)).ShouldBe(customValidation ?? command);
        }

        public void Dispose()
        {
            proto?.Dispose();
            sim?.Dispose();
        }
    }
}