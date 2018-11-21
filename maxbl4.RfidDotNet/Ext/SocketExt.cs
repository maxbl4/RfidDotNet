using System.Net.Sockets;

namespace maxbl4.RfidDotNet.Ext
{
    public static class SocketExt
    {
        public static void CloseForce(this Socket socket)
        {
            try { socket?.Shutdown(SocketShutdown.Both); } catch {}
            try { socket?.Close(); } catch {}
        }
    }
}