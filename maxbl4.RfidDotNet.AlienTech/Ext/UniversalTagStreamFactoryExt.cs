namespace maxbl4.RfidDotNet.AlienTech.Ext
{
    public static class UniversalTagStreamFactoryExt
    {
        public static void UseAlienProtocol(this UniversalTagStreamFactory factory)
        {
            factory.Register<ReconnectingAlienReaderProtocol>(ReaderProtocolType.Alien);
        }
    }
}