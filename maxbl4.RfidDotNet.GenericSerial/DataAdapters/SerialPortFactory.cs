using System.IO;
using maxbl4.RfidDotNet.Ext;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using maxbl4.RfidDotNet.GenericSerial.Model;
using RJCP.IO.Ports;

namespace maxbl4.RfidDotNet.GenericSerial.DataAdapters
{
    public class SerialPortFactory : IDataStreamFactory
    {
        public const int DefaultBaudRate = 57600;
        public const int DefaultDataBits = 8;
        public const Parity DefaultParity = Parity.None;
        public const StopBits DefaultStopBits =  StopBits.One;
        
        public string SerialPortName { get; }
        public int BaudRate { get; private set; }
        public int DataBits { get; }
        public Parity Parity { get; }
        public StopBits StopBits { get; }
        
        private SerialPortStream stream = null;

        public SerialPortFactory(string serialPortName, int baudRate = DefaultBaudRate,
            int dataBits = DefaultDataBits, Parity parity = DefaultParity, StopBits stopBits = DefaultStopBits)
        {
            SerialPortName = serialPortName;
            BaudRate = baudRate;
            DataBits = dataBits;
            Parity = parity;
            StopBits = stopBits;
        }

        public Stream DataStream 
        {
            get
            {
                if (stream == null)
                {
                    stream = new SerialPortStream(SerialPortName, BaudRate, DataBits, Parity, StopBits)
                    {
                        ReadTimeout = 3000, WriteTimeout = 200
                    };
                    stream.Open();
                }
                stream.DiscardInBuffer();
                stream.DiscardOutBuffer();
                return stream;
            }
        }

        public string Description => SerialPortName;

        public void Invalidate()
        {
            DisposableExt.DisposeSafe(stream);
            stream = null;
        }

        public void UpdateBaudRate(int baudRate)
        {
            BaudRate = baudRate;
            Invalidate();
        }

        public void Dispose()
        {
            Invalidate();
        }
    }
}