using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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

            // TODO This code is a mess smh, at the very least needs comments
            if (bytesRead > 0)
            {
                int packetStart = -1;

                if (!tcpClient.AwaitingData)
                {
                    for (int i = 0; i < tcpClient.Buffer.Length; i++)
                    {
                        if (!TCPPacket.ByteEquals(tcpClient.Buffer, TCPPacket.GetBinaryHeader(), i))
                        {
                            continue;
                        }

                        packetStart = i;
                    }

                    if (packetStart == -1)
                    {
                        // Assume invalid or corrupted pack sent.
                        return;
                    }
                }
                else
                {
                    packetStart = 0;
                }

                for (int i = packetStart; i < tcpClient.Buffer.Length; i++)
                {
                    if (tcpClient.RequestingHeader)
                    {
                        if (TCPPacket.ByteEquals(tcpClient.Buffer, TCPPacket.GetBinaryHeader(), i))
                        {
                            tcpClient.RequestingHeader = false;
                        }
                        else
                        {
                            break;
                        }
                    }

                    tcpClient.AwaitingData = false;

                    if (!TCPPacket.ByteEquals(tcpClient.Buffer, TCPPacket.GetBinaryFooter(), i))
                    {
                        tcpClient.AwaitingData = true;

                        tcpClient.InProgressPacketBytes.Add(tcpClient.Buffer[i]);

                        Debug.WriteLine($"[DEBUG] {i + 1} bytes read out of 1024. Length of in progress byte list: {tcpClient.InProgressPacketBytes.Count}");

                        continue;
                    }

                    tcpClient.RequestingHeader = true;

                    i += TCPPacket.GetBinaryFooter().Length;

                    PacketReceived?.Invoke(tcpClient, new PacketReceivedEventArgs
                    {
                        TCPPacket = new TCPPacket(tcpClient.InProgressPacketBytes.ToArray()),
                        Sender = tcpClient
                    });

                    tcpClient.InProgressPacketBytes.Clear();
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