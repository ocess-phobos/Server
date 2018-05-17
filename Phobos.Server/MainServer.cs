using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Phobos.Server.Sockets;
using Phobos.Server.Sockets.Handlers;
using Phobos.Server.Sockets.Packets;

namespace Phobos.Server
{
    public class MainServer
    {
        public MainServer()
        {
            // Subscribe to the packet event
            AsynchronousSocketListener.PacketReceived += this.AsynchronousSocketListener_PacketRecieved;
        }

        /// <summary>
        /// Handles packets received from client(s)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AsynchronousSocketListener_PacketRecieved(object sender, PacketReceivedEventArgs e)
        {
            switch (e.TCPPacket.TextHeader.ToLower())
            {
                case "testimage":
                    {
                        Debug.WriteLine(e.TCPPacket.DataBytesList.Count);

                        File.WriteAllBytes(Path.Combine(Globals.AppPath, "test.png"), e.TCPPacket.DataBytesList[0].ToArray());
                        //Process.Start(Path.Combine(Globals.AppPath, "test.png"));
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Send data to clients
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="packet"></param>
        public static void Send(TCPClient handler, TCPPacket packet)
        {
            byte[] buffer = TCPPacket.GetBinaryHeader()
                .Concat(BitConverter.GetBytes(packet.TextHeader.Length))
                .Concat(Encoding.UTF8.GetBytes(packet.TextHeader))
                .Concat(packet.DataBytesList.Aggregate((a1, a2) => a1.Concat(Encoding.UTF8.GetBytes(TCPPacket.SEP_STRING)).Concat(a2).ToArray()))
                .Concat(TCPPacket.GetBinaryFooter()).ToArray();

            for (int i = 0; i < buffer.Length; i += 1024)
            {
                // Begin sending the data to the remote device.  
                handler.WorkSocket.BeginSend(buffer, 0, buffer.Length <= 1024 ? buffer.Length - i : 1024, 0, SendData, handler.WorkSocket);
            }
        }

        private static void SendData(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine($"Sent {bytesSent} bytes to client.");

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
