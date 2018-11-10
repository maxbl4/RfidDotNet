using System;

namespace maxbl4.RfidDotNet.AlienTech.Enums
{
    public class TagStreamServer
    {
        public const int DefaultPort = 3333;
        /// <summary>
        /// 0 - disable server. Max 16 clients
        /// </summary>
        public int AllowedClients { get; }
        public int Port { get; }
        public override string ToString()
        {
            return $"{AllowedClients} {Port} any";
        }

        public TagStreamServer(string src)
        {
            var parts = src.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                throw new Exception($"Unexpected data {src}");
            AllowedClients = int.Parse(parts[0]);
            Port = int.Parse(parts[1]);
        }

        public TagStreamServer(int allowedClients, int port = DefaultPort)
        {
            AllowedClients = allowedClients;
            Port = port;
        }
    }
}