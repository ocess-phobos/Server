using System.Collections.Generic;
using System.Net.Sockets;

namespace Phobos.Server.Sockets
{
    /// <summary>
    /// State object for reading client data asynchronously
    /// </summary>
    public class TCPClient
    {
        // Size of receive buffer.  
        public const int BUFFER_SIZE = 1024;
        public int CurrentPacketLength;
        public bool AwaitingData;
        public Socket WorkSocket { get; set; }
        // Receive buffer.  
        public byte[] Buffer = new byte[BUFFER_SIZE];
        // Received data string.  
        public List<byte> InProgressPacketBytes = new List<byte>();
    }
}
