using System;
using System.Threading.Tasks;

namespace maxbl4.RfidDotNet
{
    public interface IUniversalTagStream : IDisposable
    {
        IObservable<Tag> Tags { get; }
        IObservable<Exception> Errors { get; }
        IObservable<bool> Connected { get; }
        Task Start();
        Task Stop();
        Task<int> QValue(int? newValue = null);
        Task<int> Session(int? newValue = null);
        Task<int> RFPower(int? newValue = null);
        Task<AntennaConfiguration> AntennaConfiguration(AntennaConfiguration? newValue = null);
    }
}