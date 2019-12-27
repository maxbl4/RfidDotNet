using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.AlienTech.Tests.Settings;
using maxbl4.RfidDotNet.Exceptions;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests
{
    public class AlienReaderProtocolTests : ReaderFixture
    {
        [Fact]
        public void Connect_timeout()
        {
            if (Settings.UseHardwareReader) return;
            Simulator.OnClientAccepted = x => Thread.Sleep(10000);
            var sw = Stopwatch.StartNew();
            Assert.ThrowsAny<Exception>(() =>
                new AlienReaderProtocol().ConnectAndLogin(Host, Port, "alien", "password", 100).Wait());
            sw.Stop();
        }
        
        [Fact]
        public void Login()
        {
        }
        
        [Fact]
        public async Task RfModulation()
        {
            (await Proto.SendReceive("RFModulation = DRM")).Should().Be("RFModulation = DRM");
            (await Proto.SendReceive("RFModulation?")).Should().Be("RFModulation = DRM");
        }

        [Fact]
        public void LoginWithWrongPassword()
        {
            Proto?.Dispose();
            Proto = new AlienReaderProtocol();
            Assert.Throws<AggregateException>(() => Proto.ConnectAndLogin(Host, Port, "alien", "password1").Wait())
                .InnerException.Should().BeOfType<LoginFailedException>();
        }
        
        [Fact]
        public async Task SetupReader()
        {
            await SendReceiveConfirm("TagListMillis = ON");
            await SendReceiveConfirm("RFModulation = DRM");
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
            (await Proto.SendReceive("AutoModeReset")).Should().Be(ProtocolMessages.AutoModeResetConfirmation);
        }

        [Fact]
        public async Task Clear()
        {
            (await Proto.SendReceive("Clear")).Should().Be(ProtocolMessages.TagListClearConfirmation);
        }

        async Task SendReceiveConfirm(string command, string customValidation = null)
        {
            (await Proto.SendReceive(command)).Should().Be(customValidation ?? command);
        }
    }
}