using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace maxbl4.RfidDotNet
{
    public class UniversalTagStreamFactory
    {
        private Dictionary<ReaderProtocolType, Type> implementations = new Dictionary<ReaderProtocolType, Type>();
        public void Register<T>(ReaderProtocolType protocolType)
            where T:IUniversalTagStream
        {
            implementations[protocolType] = typeof(T);
        }

        public IUniversalTagStream Create(string connectionString)
        {
            return Create(ConnectionString.Parse(connectionString));
        }

        public IUniversalTagStream Create(ConnectionString connectionString)
        {
            if (!implementations.ContainsKey(connectionString.Protocol))
                throw new ArgumentOutOfRangeException(nameof(connectionString), $"No implementation for {connectionString.Protocol} registered");
            if (!connectionString.IsValid(out var msg))
                throw new ArgumentException(msg, nameof(connectionString));
            var implType = implementations[connectionString.Protocol];
            return (IUniversalTagStream)Activator.CreateInstance(implType, connectionString);
        }
    }
}