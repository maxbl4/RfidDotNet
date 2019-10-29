using System.Collections.Generic;
using System.Net;

namespace maxbl4.RfidDotNet.AlienTech.Tests.Settings
{
    public class AlienTestSettings
    {
        public bool UseHardwareReader { get; set; }
        public string HardwareReaderAddress { get; set; } 
        public string ReaderSimAddress { get; set; }
        public List<string> KnownTagIds { get; set; }
    }
}