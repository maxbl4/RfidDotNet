namespace maxbl4.RfidDotNet.AlienTech.Extensions
{
    public static class UniversalTagStreamFactoryExt
    {
        public static void UseAlienProtocol(this UniversalTagStreamFactory factory)
        {
            factory.Register(ReaderProtocolType.Alien, x => new ReconnectingAlienReaderProtocol(x));
        }
    }
}