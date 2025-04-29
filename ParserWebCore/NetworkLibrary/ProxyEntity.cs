#region

using System;

#endregion

namespace ParserWebCore.NetworkLibrary
{
    public class ProxyEntity
    {
        public ProxyEntity(string ip, int port, string user, string pass)
        {
            Ip = ip ?? throw new ArgumentNullException(nameof(ip));
            Port = port;
            User = user ?? throw new ArgumentNullException(nameof(ip));
            Pass = pass ?? throw new ArgumentNullException(nameof(ip));
        }

        public string Ip { get; }
        public int Port { get; }
        public string User { get; }
        public string Pass { get; }
    }
}