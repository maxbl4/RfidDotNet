using System;
using maxbl4.RfidDotNet.GenericSerial.DataAdapters;

namespace maxbl4.RfidDotNet.GenericSerial
{
    public class SerialConnectionString
    {
        public ConnectionString ConnectionString { get; private set; }

        public SerialConnectionString(ConnectionString cs)
        {
            this.ConnectionString = cs;
        }
        
        public ConnectionType Type 
        {
            get
            {
                if (ConnectionString.IsValid(out var msg) && ConnectionString.Protocol == ReaderProtocolType.Serial)
                {
                    if (ConnectionString.Network != null)
                        return ConnectionType.Network;
                    if (ConnectionString.Serial != null)
                        return ConnectionType.Serial;
                }
                return ConnectionType.None;
            }
        }

        public IDataStreamFactory Connect()
        {
            switch (Type)
            {
                case ConnectionType.Serial:
                    return new SerialPortFactory(ConnectionString.Serial.Port, ConnectionString.Serial.BaudRate);
                case ConnectionType.Network:
                    return new NetworkStreamFactory(ConnectionString.Network);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}