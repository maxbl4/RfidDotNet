using System;
using System.Threading.Tasks;

namespace maxbl4.RfidDotNet
{
    public interface IUniversalTagStream : IDisposable
    {
        IObservable<Tag> Tags { get; }
        IObservable<Exception> Errors { get; }
        IObservable<bool> Connected { get; }
        IObservable<DateTime> Heartbeat { get; }
        Task Start();
        bool Start2();
        Task<int> QValue(int? newValue = null);
        Task<int> Session(int? newValue = null);
        Task<int> RFPower(int? newValue = null);
        Task<AntennaConfiguration> AntennaConfiguration(AntennaConfiguration? newValue = null);
    }
}