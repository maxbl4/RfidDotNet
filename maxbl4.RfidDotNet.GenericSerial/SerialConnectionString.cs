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
                if (ConnectionString.IsValid(out var msg))
                {
                    if (!string.IsNullOrEmpty(ConnectionString.TcpHost))
                        return ConnectionType.Network;
                    if (!string.IsNullOrEmpty(ConnectionString.SerialPortName))
                        return ConnectionType.Network;
                }
                return ConnectionType.None;
            }
        }

        public IDataStreamFactory Connect()
        {
            switch (Type)
            {
                case ConnectionType.Serial:
                    return new SerialPortFactory(ConnectionString.SerialPortName, ConnectionString.SerialBaudRate);
                case ConnectionType.Network:
                    return new NetworkStreamFactory(ConnectionString.TcpHost, ConnectionString.TcpPort);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}