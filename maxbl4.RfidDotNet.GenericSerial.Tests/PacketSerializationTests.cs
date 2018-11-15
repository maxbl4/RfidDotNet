using maxbl4.RfidDotNet.GenericSerial.Packets;
using Shouldly;
using Xunit;

namespace maxbl4.RfidDotNet.GenericSerial.Tests
{
    public class PacketSerializationTests
    {
        //Get serial
        //04 00 4c 3a d2  
        //Response should be
        //09 00 4c 00 17 43 90 15 49 c0
            
        //Get info
        //04 00 21 d9 6a
        //Response
        //11 00 21 00 03 01 10 02 31 80 1a 03 01 00 00 00 af bf
            
        //Set beep = true
        //05 00 40 01 99 3a
        //05 00 40 00 10 2b
        //beep = false
        //05 00 40 00 10 2b
        //05 00 40 00 10 2b         
            
        //Read tag buffer 
//            04 00 72 c7 0a                                    ..rЗ.            
//                Data 
//            16 00 72 01 01 01 0c e2 00 00 17 38 04 00 71 11   ..r....в...8..q. 
//            90 9e e5 d0 03 9d 84                              ђћеР.ќ„          

        
        [Fact]
        public void Get_reader_serial_packet_data()
        {
            var pk = new CommandDataPacket(ReaderCommand.GetReaderSerialNumber);
            var buf = new byte[100];
            var len = pk.Serialize(buf);
            len.ShouldBe(5);
            buf[0].ShouldBe((byte)4);
            buf[1].ShouldBe((byte)0);
            buf[2].ShouldBe((byte)ReaderCommand.GetReaderSerialNumber);
            buf[3].ShouldBe((byte)0x3a);
            buf[4].ShouldBe((byte)0xd2);
        }
    }
}