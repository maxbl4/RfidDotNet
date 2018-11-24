using System;

namespace maxbl4.RfidDotNet.GenericSerial.Model
{
    //TODO: Implement support for Mask memory
    public class RealtimeInventoryParams
    {
        public const byte DefaultQValue = 4;
        public const SessionValue DefaultSession = SessionValue.S0;
        public const RealtimeProtocolType DefaultTagProtocol = RealtimeProtocolType.Gen18000_6C;
        public const ReadPauseTimes DefaultReadPauseTime = ReadPauseTimes.Pause10ms;
        public const int DefaultTagDebounceTimeSec = 1;

        /// <summary>
        /// Set tag protocol.
        /// Configuration of QValue, Session, MaskMem, MaskAdr, MaskLen, MaskData, AdrTID, LenTID
        /// are ignored for 6B protocol.
        /// </summary>
        public RealtimeProtocolType TagProtocol { get; set; } = DefaultTagProtocol;

        /// <summary>
        /// Pause between inventories
        /// </summary>
        public ReadPauseTimes ReadPauseTime { get; set; } = DefaultReadPauseTime;

        /// <summary>
        /// Reader will ignore same tag ids read within this interval.
        /// Range 0 - 255 seconds. 0 - disable filtering. Default 1s
        /// </summary>
        public TimeSpan TagDebounceTime { get; set; } = TimeSpan.FromSeconds(DefaultTagDebounceTimeSec);

        /// <summary>
        /// Range is 0-15. The value should be: 2^Q = Expected number of tags in field
        /// </summary>
        public byte QValue { get; set; } = DefaultQValue;

        /// <summary>
        /// Reader specific flags
        /// </summary>
        public QFlags QFlags { get; set; } = QFlags.None;

        public SessionValue Session { get; set; } = DefaultSession;

        public byte[] Serialize()
        {
            var offset = 0;
            var buf = new byte[5];
            buf[offset++] = (byte)TagProtocol;
            buf[offset++] = (byte)ReadPauseTime;
            buf[offset++] = (byte)TagDebounceTime.TotalSeconds;
            buf[offset++] = (byte)(QValue | (byte)QFlags);
            buf[offset++] = (byte) Session;
            return buf;
        }
    }
}