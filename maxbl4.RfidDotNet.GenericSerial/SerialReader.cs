using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.Ext;
using maxbl4.RfidDotNet.GenericSerial.Packets;
using RJCP.IO.Ports;

namespace maxbl4.RfidDotNet.GenericSerial
{
    public class SerialReader : IDisposable
    {
        public const int DefaultPortSpeed = 57600;
        private readonly SemaphoreSlim sendReceiveSemaphore = new SemaphoreSlim(1);
        private SerialPortStream port;
        private byte[] buffer = new byte[1000000];

        public SerialReader(string serialPortName)
        {
            this.port = new SerialPortStream(serialPortName, DefaultPortSpeed, 8, Parity.None, StopBits.One);
            port.ReadTimeout = 3000;
            port.WriteTimeout = 200;
            port.Open();
        }

        public async Task<T> SendReceive<T>(CommandDataPacket command)
            where T: ResponseDataPacket
        {
            using (sendReceiveSemaphore.UseOnce())
            {
                var length = command.Serialize(buffer);
                await port.WriteAsync(buffer, 0, length);
                return null;
            }
        }

        public async Task<ResponseDataPacket> ReceicePacket()
        {
            var read = await port.ReadAsync(buffer, 0, buffer.Length);
            return null;
        }

        public int GetSerialNumber()
        {
            port.Write(new byte[]{0x04, 0x00, 0x4c, 0x3a, 0xd2}, 0, 5);
            var b = new byte[10];
            return port.Read(b, 0, 10);
        }

        public void Dispose()
        {
            port?.Close();
            port?.Dispose();
        }
    }
}