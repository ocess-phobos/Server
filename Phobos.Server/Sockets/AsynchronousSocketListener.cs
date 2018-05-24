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
                    listener.BeginAccept(AcceptCallback, listener);

                    // Wait until a connection is made before continuing.  
                    AllDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nAll listeners closed. Press ENTER to close...");
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
            PhobosClient state = new PhobosClient
            {
                WorkSocket = handler
            };

            handler.BeginReceive(state.Buffer, 0, PhobosClient.BUFFER_SIZE, 0, ReadCallback, state);
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            PhobosClient phobosClient = (PhobosClient)ar.AsyncState;
            Socket handler = phobosClient.WorkSocket;

            // Read data from the client socket.   
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                using (MemoryStream stream = new MemoryStream(phobosClient.Buffer))
                using (EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, stream))
                {
                    while (reader.BaseStream.Position < phobosClient.Buffer.Length)
                    {
                        phobosClient.AwaitingData = true;

                        if (phobosClient.CurrentPacketLength <= 0)
                        {
                            phobosClient.CurrentPacketLength = reader.ReadInt32();

                            if (phobosClient.CurrentPacketLength <= 0)
                            {
                                break;
                            }

                            phobosClient.InProgressPacketBytes.AddRange(EndianBitConverter.Big.GetBytes(phobosClient.CurrentPacketLength));
                        }

                        int remainingBytes = phobosClient.CurrentPacketLength - phobosClient.InProgressPacketBytes.Count;

                        // ReadBytes only returns the available amount of bytes
                        phobosClient.InProgressPacketBytes.AddRange(reader.ReadBytes((remainingBytes > 1024 ? 1024 : remainingBytes) < 0 ? 1024 : remainingBytes));

                        if (phobosClient.CurrentPacketLength != phobosClient.InProgressPacketBytes.Count)
                        {
                            continue;
                        }

                        PacketReceived?.Invoke(phobosClient, new PacketReceivedEventArgs
                        {
                            PhobosPacket = new PhobosPacket(phobosClient.InProgressPacketBytes.ToArray()),
                            Sender = phobosClient
                        });

                        phobosClient.InProgressPacketBytes.Clear();

                        phobosClient.CurrentPacketLength = -1;
                        phobosClient.AwaitingData = false;
                    }
                }

                phobosClient.Buffer = new byte[1024];

                if (phobosClient.AwaitingData)
                {
                    handler.BeginReceive(phobosClient.Buffer, 0, PhobosClient.BUFFER_SIZE, 0, ReadCallback, phobosClient);
                }
                else if (phobosClient.InProgressPacketBytes.Any())
                {
                    PacketReceived?.Invoke(phobosClient, new PacketReceivedEventArgs
                    {
                        PhobosPacket = new PhobosPacket(phobosClient.InProgressPacketBytes.ToArray()),
                        Sender = phobosClient
                    });

                    phobosClient.InProgressPacketBytes.Clear();
                }
            }
            else if (phobosClient.InProgressPacketBytes.Any())
            {

                PacketReceived?.Invoke(phobosClient, new PacketReceivedEventArgs
                {
                    PhobosPacket = new PhobosPacket(phobosClient.InProgressPacketBytes.ToArray()),
                    Sender = phobosClient
                });

                phobosClient.InProgressPacketBytes.Clear();
            }
        }
    }
}