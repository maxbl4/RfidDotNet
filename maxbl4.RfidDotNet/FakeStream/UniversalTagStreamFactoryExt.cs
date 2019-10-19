using System;

namespace maxbl4.RfidDotNet.AlienTech.Ext
{
    public static class UniversalTagStreamFactoryExt
    {
        public static void UseFakeStream(this UniversalTagStreamFactory factory, Func<ConnectionString, IUniversalTagStream> fakeStreamFactory)
        {
            factory.Register(ReaderProtocolType.Fake, fakeStreamFactory);
        }
    }
}