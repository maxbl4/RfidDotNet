using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using maxbl4.Infrastructure;
using Xunit;

namespace maxbl4.RfidDotNet.AlienTech.Tests
{
    public class TimeoutActionTests
    {
        [Fact]
        public void Should_execute_after_timeout()
        {
            var sw = Stopwatch.StartNew();
            using (TimeoutAction.Set(100, () => sw.Stop()))
                Thread.Sleep(2000);
            sw.ElapsedMilliseconds.Should().BeInRange(0, 150);
        }
    }
}