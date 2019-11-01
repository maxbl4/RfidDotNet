using System.Net;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests.ReaderSimulator
{
    public class SimulatorListenerTests
    {
        [Fact]
        public void ShouldSetHandlerProperly()
        {
            var sim = new SimulatorListener(IPEndPoint.Parse("127.0.0.1:20023"));
            sim.TagListHandler = () => "";
        }
    }
}