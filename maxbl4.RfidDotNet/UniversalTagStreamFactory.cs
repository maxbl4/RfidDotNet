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
            if (!implementations.ContainsKey(connectionString.ProtocolType))
                throw new ArgumentOutOfRangeException(nameof(connectionString), $"No implementation for {connectionString.ProtocolType} registered");
            var implType = implementations[connectionString.ProtocolType];
            return (IUniversalTagStream)Activator.CreateInstance(implType, connectionString);
        }
    }
}