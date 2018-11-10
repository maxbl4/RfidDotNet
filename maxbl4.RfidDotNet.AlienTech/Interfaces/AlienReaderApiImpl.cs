using System;
using System.Threading.Tasks;

namespace maxbl4.RfidDotNet.AlienTech.Interfaces
{
    public class AlienReaderApiImpl : AlienReaderApi
    {
        private readonly Func<string, Task<string>> sendReceiveImpl;

        public AlienReaderApiImpl(Func<string, Task<string>> sendReceiveImpl)
        {
            this.sendReceiveImpl = sendReceiveImpl;
        }

        public override Task<string> SendReceive(string command)
        {
            return sendReceiveImpl(command);
        }
    }
}