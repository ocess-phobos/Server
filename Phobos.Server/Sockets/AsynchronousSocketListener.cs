using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MiscUtil.Core.Conversion;
using MiscUtil.Core.IO;
using Phobos.Server.Sockets.Handlers;
using Phobos.Server.Sockets.Packets;

namespace Phobos.Server.Sockets
{
    public static class AsynchronousSocketListener
    {
        /// <summary>
        /// Event fired when a packet has been successfully received.
        /// </summary>
        public static event EventHandler<PacketReceivedEventArgs> PacketReceived;

        // Thread signal.  
        private static readonly ManualResetEvent AllDone = new ManualResetEvent(false);

        public static void StartListening()
        {
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to non-signaled state.  
                    AllDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(AcceptCallback, listener);

                    // Wait until a connection is made before continuing.  
                    AllDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            AllDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            TCPClient state = new TCPClient
            {
                WorkSocket = handler
            };

            handler.BeginReceive(state.Buffer, 0, TCPClient.BUFFER_SIZE, 0, ReadCallback, state);
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            TCPClient tcpClient = (TCPClient)ar.AsyncState;
            Socket handler = tcpClient.WorkSocket;

            // Read data from the client socket.   
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                if (!File.Exists(Path.Combine(Globals.AppPath, "test.bin")))
                {
                    File.Create(Path.Combine(Globals.AppPath, "test.bin")).Dispose();
                }

                using (MemoryStream stream = new MemoryStream(tcpClient.Buffer))
                using (EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, stream))
                {
                    if (tcpClient.CurrentPacketLength == -1)
                    {
                        tcpClient.CurrentPacketLength = reader.ReadInt32();

                        tcpClient.InProgressPacketBytes.AddRange(EndianBitConverter.Big.GetBytes(tcpClient.CurrentPacketLength));
                    }

                    while (reader.BaseStream.Position < tcpClient.Buffer.Length)
                    {
                        tcpClient.AwaitingData = true;

                        if (tcpClient.Buffer.Length - reader.BaseStream.Position < 4)
                        {
                            break;
                        }

                        int chunkLength = reader.ReadInt32();

                        if (chunkLength == 0)
                        {
                            tcpClient.AwaitingData = false;
                            break;
                        }

                        reader.BaseStream.Position -= 4;

                        tcpClient.InProgressPacketBytes.AddRange(reader.ReadBytes(4 + chunkLength));

                        File.WriteAllBytes(Path.Combine(Globals.AppPath, "test.bin"), tcpClient.InProgressPacketBytes.ToArray());
                    }

                    if (tcpClient.CurrentPacketLength == tcpClient.InProgressPacketBytes.Count)
                    {
                        tcpClient.AwaitingData = false;
                    }
                }

                tcpClient.Buffer = new byte[1024];

                if (tcpClient.AwaitingData)
                {
                    handler.BeginReceive(tcpClient.Buffer, 0, TCPClient.BUFFER_SIZE, 0, ReadCallback, tcpClient);
                }
                else if (tcpClient.InProgressPacketBytes.Any())
                {
                    PacketReceived?.Invoke(tcpClient, new PacketReceivedEventArgs
                    {
                        TCPPacket = new TCPPacket(tcpClient.InProgressPacketBytes.ToArray()),
                        Sender = tcpClient
                    });

                    tcpClient.InProgressPacketBytes.Clear();
                    tcpClient.CurrentPacketLength = -1;
                }
            }
            else if (tcpClient.InProgressPacketBytes.Any())
            {

                PacketReceived?.Invoke(tcpClient, new PacketReceivedEventArgs
                {
                    TCPPacket = new TCPPacket(tcpClient.InProgressPacketBytes.ToArray()),
                    Sender = tcpClient
                });

                tcpClient.InProgressPacketBytes.Clear();
            }
        }
    }
}