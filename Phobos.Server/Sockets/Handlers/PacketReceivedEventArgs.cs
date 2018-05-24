using System;
using Phobos.Server.Sockets.Packets;

namespace Phobos.Server.Sockets.Handlers
{
    public class PacketReceivedEventArgs : EventArgs
    {
        public PhobosPacket PhobosPacket { get; set; }
        public PhobosClient Sender { get; set; }
    }
}
