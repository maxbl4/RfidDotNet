using System.IO;
using System.IO.Ports;
using maxbl4.Infrastructure.Extensions.DisposableExt;

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
        
        private SerialPort stream = null;

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
                    stream = new SerialPort(SerialPortName, BaudRate, Parity, DataBits, StopBits)
                    {
                        ReadTimeout = 3000, WriteTimeout = 200
                    };
                    stream.Open();
                }
                stream.DiscardInBuffer();
                stream.DiscardOutBuffer();
                return stream.BaseStream;
            }
        }

        public string Description => SerialPortName;

        public void Invalidate()
        {
            stream.DisposeSafe();
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