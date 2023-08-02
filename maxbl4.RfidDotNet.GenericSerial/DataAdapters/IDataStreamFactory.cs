using System;
using System.IO;
using System.IO.Ports;

namespace maxbl4.RfidDotNet.GenericSerial.DataAdapters
{
    public interface IDataStreamFactory : IDisposable
    {
        Stream DataStream { get; }
        string Description { get; }
        void Invalidate();
        void UpdateBaudRate(int baudRate);
    }
}