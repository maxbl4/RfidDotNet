using System;
using System.IO;

namespace maxbl4.RfidDotNet.GenericSerial.DataAdapters
{
    public interface IDataStreamFactory : IDisposable
    {
        Stream DataStream { get; }
        string Description { get; }
        void Invalidate();
    }
}