namespace maxbl4.RfidDotNet.GenericSerial.Ext
{
    public static class UniversalTagStreamFactoryExt
    {
        public static void UseSerialProtocol(this UniversalTagStreamFactory factory)
        {
            factory.Register(ReaderProtocolType.Serial, x => new SerialUnifiedTagStream(x));
        }

        public static void UseSl130Protocol(this UniversalTagStreamFactory factory)
        {
            factory.Register(ReaderProtocolType.Sl130, x => new Sl130.Sl130UnifiedTagStream(x));
        }
    }
}