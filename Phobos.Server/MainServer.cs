using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using MiscUtil.Core.Conversion;
using MiscUtil.Core.IO;
using Phobos.Server.Clients.Enum;
using Phobos.Server.Helpers;
using Phobos.Server.Sockets;
using Phobos.Server.Sockets.Handlers;
using Phobos.Server.Sockets.Packets;

namespace Phobos.Server
{
    public class MainServer
    {
        public DateTime SimDateTime { get; set; }

        private readonly Point timestampPoint;

        public MainServer()
        {
            Console.CursorVisible = false;

            ConsoleHelper.WriteLine("-------PHOBOS/EECOM Server-------", ConsoleColor.DarkCyan);
            ConsoleHelper.Write("Flight:            ", ConsoleColor.Cyan);
            ConsoleHelper.WriteLine("Not Connected.", ConsoleColor.Red);
            ConsoleHelper.Write("Mirror:            ", ConsoleColor.Cyan);
            ConsoleHelper.WriteLine("Not Connected.", ConsoleColor.Red);
            ConsoleHelper.Write("Telemetry:         ", ConsoleColor.Cyan);
            ConsoleHelper.WriteLine("Not Connected.", ConsoleColor.Red);
            ConsoleHelper.Write("Simulator:         ", ConsoleColor.Cyan);
            ConsoleHelper.WriteLine("Not Connected.", ConsoleColor.Red);
            ConsoleHelper.Write("Display:           ", ConsoleColor.Cyan);
            ConsoleHelper.WriteLine("Not Connected.", ConsoleColor.Red);
            ConsoleHelper.Write("Habitat EECOM:     ", ConsoleColor.Cyan);
            ConsoleHelper.WriteLine("Not Connected.", ConsoleColor.Red);
            ConsoleHelper.Write("MC EECOM:          ", ConsoleColor.Cyan);
            ConsoleHelper.WriteLine("Not Connected.", ConsoleColor.Red);
            ConsoleHelper.Write("Simulation EECOM:  ", ConsoleColor.Cyan);
            ConsoleHelper.WriteLine("Not Connected.", ConsoleColor.Red);
            ConsoleHelper.Write("Habitat Engine:    ", ConsoleColor.Cyan);
            ConsoleHelper.WriteLine("Not Connected.", ConsoleColor.Red);
            ConsoleHelper.Write("Simulation Mirror: ", ConsoleColor.Cyan);
            ConsoleHelper.WriteLine("Not Connected.", ConsoleColor.Red);
            ConsoleHelper.Write("Habitat Display:   ", ConsoleColor.Cyan);
            ConsoleHelper.WriteLine("Not Connected.", ConsoleColor.Red);
            Console.WriteLine("\n");

            this.SimDateTime = new DateTime(2069, 3, 6, 21, 42, 0);

            this.timestampPoint = new Point(Console.CursorLeft, Console.CursorTop);
            
            Timer timestampTimer = new Timer(1000);
            timestampTimer.Elapsed += this.TimestampTimer_Elapsed;
            timestampTimer.Start();

            // Subscribe to the packet event
            AsynchronousSocketListener.PacketReceived += this.AsynchronousSocketListener_PacketRecieved;
        }

        private void TimestampTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.SetCursorPosition(this.timestampPoint.X, this.timestampPoint.Y);

            this.SimDateTime = this.SimDateTime.AddSeconds(1);

            ConsoleHelper.WriteLine(this.SimDateTime + "                ", ConsoleColor.Cyan);
        }

        /// <summary>
        /// Send data to clients
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="packet"></param>
        public static void Send(PhobosClient handler, PhobosPacket packet)
        {
            byte[] buffer = null; // TODO REWRITE

            for (int i = 0; i < buffer.Length; i += 1024)
            {
                // Begin sending the data to the remote device.  
                handler.WorkSocket.BeginSend(buffer, i, buffer.Length <= 1024 ? buffer.Length - i : 1024, 0, SendData, handler.WorkSocket);
            }
        }

        /// <summary>
        /// Handles packets received from client(s)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AsynchronousSocketListener_PacketRecieved(object sender, PacketReceivedEventArgs e)
        {
            switch (e.PhobosPacket.TextHeader.ToLower())
            {
                case "phobos":
                {
                    using (MemoryStream stream = new MemoryStream(e.PhobosPacket.RawDataBytes))
                    using (EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, stream))
                    {
                        Console.WriteLine($"{reader.ReadUInt32()}");

                        int textLength = reader.ReadInt32();

                        Console.WriteLine($"{Encoding.ASCII.GetString(reader.ReadBytes(textLength))}");

                        Console.WriteLine($"{EndianBitConverter.Little.ToDouble(e.PhobosPacket.RawDataBytes, (int)reader.BaseStream.Position)}");
                    }
                }
                    break;
                case "flight":
                {
                    this.Connect(ClientType.Flight);

                    e.Sender.ClientType = ClientType.Flight;
                }
                    break;
                case "testimage":
                {
                    using (MemoryStream stream = new MemoryStream(e.PhobosPacket.RawDataBytes))
                    using (EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, stream))
                    {
                        int image1Length = reader.ReadInt32();

                        File.WriteAllBytes(Path.Combine(Globals.AppPath, "test1.png"), reader.ReadBytes(image1Length));

                        int image2Length = reader.ReadInt32();

                        File.WriteAllBytes(Path.Combine(Globals.AppPath, "test2.png"), reader.ReadBytes(image2Length));
                    }
                }
                    break;
                default:
                    break;
            }
        }

        private void Connect(ClientType clientType)
        {
            Point originalCursorPoint = new Point(Console.CursorLeft, Console.CursorTop);

            switch (clientType)
            {
                case ClientType.Flight:
                    Console.SetCursorPosition(19, 1);
                    ConsoleHelper.WriteLine("Connected.    ", ConsoleColor.Green);
                    break;
                case ClientType.Mirror:
                    Console.SetCursorPosition(19, 2);
                    ConsoleHelper.WriteLine("Connected.    ", ConsoleColor.Green);
                    break;
                case ClientType.Telemetry:
                    Console.SetCursorPosition(19, 3);
                    ConsoleHelper.WriteLine("Connected.    ", ConsoleColor.Green);
                    break;
                case ClientType.Simulator:
                    Console.SetCursorPosition(19, 4);
                    ConsoleHelper.WriteLine("Connected.    ", ConsoleColor.Green);
                    break;
                case ClientType.Display:
                    Console.SetCursorPosition(19, 5);
                    ConsoleHelper.WriteLine("Connected.    ", ConsoleColor.Green);
                    break;
                case ClientType.HabitatEECOM:
                    Console.SetCursorPosition(19, 6);
                    ConsoleHelper.WriteLine("Connected.    ", ConsoleColor.Green);
                    break;
                case ClientType.MCEECOM:
                    Console.SetCursorPosition(19, 7);
                    ConsoleHelper.WriteLine("Connected.    ", ConsoleColor.Green);
                    break;
                case ClientType.SimulationEECOM:
                    Console.SetCursorPosition(19, 8);
                    ConsoleHelper.WriteLine("Connected.    ", ConsoleColor.Green);
                    break;
                case ClientType.HabitatEngine:
                    Console.SetCursorPosition(19, 9);
                    ConsoleHelper.WriteLine("Connected.    ", ConsoleColor.Green);
                    break;
                case ClientType.SimulationMirror:
                    Console.SetCursorPosition(19, 10);
                    ConsoleHelper.WriteLine("Connected.    ", ConsoleColor.Green);
                    break;
                case ClientType.HabitatDisplay:
                    Console.SetCursorPosition(19, 11);
                    ConsoleHelper.WriteLine("Connected.    ", ConsoleColor.Green);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(clientType), clientType, null);
            }

            Console.SetCursorPosition(originalCursorPoint.X, originalCursorPoint.Y);
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
