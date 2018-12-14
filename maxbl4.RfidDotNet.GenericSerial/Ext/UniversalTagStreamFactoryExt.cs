namespace maxbl4.RfidDotNet.GenericSerial.Ext
{
    public static class UniversalTagStreamFactoryExt
    {
        public static void UseSerialProtocol(this UniversalTagStreamFactory factory)
        {
            factory.Register<SerialUnifiedTagStream>(ReaderProtocolType.Serial);
        }
    }
}