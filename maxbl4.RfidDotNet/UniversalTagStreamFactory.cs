using System;
using System.Collections.Generic;

namespace maxbl4.RfidDotNet
{
    public class UniversalTagStreamFactory
    {
        private Dictionary<ReaderProtocolType, Func<ConnectionString, IUniversalTagStream>> implementations = new Dictionary<ReaderProtocolType, Func<ConnectionString, IUniversalTagStream>>();
        public void Register(ReaderProtocolType protocolType, Func<ConnectionString, IUniversalTagStream> tagStreamFactory)
        {
            implementations[protocolType] = tagStreamFactory;
        }

        public IUniversalTagStream CreateStream(string connectionString)
        {
            return CreateStream(ConnectionString.Parse(connectionString));
        }

        public IUniversalTagStream CreateStream(ConnectionString connectionString)
        {
            if (!implementations.ContainsKey(connectionString.Protocol))
                throw new ArgumentOutOfRangeException(nameof(connectionString), $"No implementation for {connectionString.Protocol} registered");
            if (!connectionString.IsValid(out var msg))
                throw new ArgumentException(msg, nameof(connectionString));
            var tagStreamFactory = implementations[connectionString.Protocol];
            return tagStreamFactory(connectionString);
        }
    }
}