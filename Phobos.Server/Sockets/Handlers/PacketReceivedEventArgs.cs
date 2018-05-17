using System;
using Phobos.Server.Sockets.Packets;

namespace Phobos.Server.Sockets.Handlers
{
    public class PacketReceivedEventArgs : EventArgs
    {
        public TCPPacket TCPPacket { get; set; }
        public TCPClient Sender { get; set; }
    }
}
