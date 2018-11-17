using System.Collections.Generic;
using maxbl4.RfidDotNet.GenericSerial.Packets;

namespace maxbl4.RfidDotNet.GenericSerial.Model
{
    public class TagInventoryResult
    {
        public List<Tag> Tags { get; }
        public TagInventoryResult(IEnumerable<ResponseDataPacket> packets)
        {
            
        }
    }
}