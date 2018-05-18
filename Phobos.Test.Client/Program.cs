using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Phobos.Server.Sockets.Packets;

namespace Phobos.Test.Client
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.SetCursorPosition(0, Console.CursorTop);

            Console.WriteLine(TCPPacket.GetBinaryFooter().Length);

            File.WriteAllText(Path.Combine(Globals.AppPath, "test.bin"), Convert.ToBase64String(File.ReadAllBytes(Path.Combine(Globals.AppPath, "test.png"))));

            TcpClient client = new TcpClient("localhost", 11000);
            NetworkStream stream = client.GetStream();

            byte[] buffer = TCPPacket.GetBinaryHeader()
                .Concat(BitConverter.GetBytes(Encoding.UTF8.GetBytes("testimage").Length))
                .Concat(Encoding.UTF8.GetBytes("testimage"))
                .Concat(Encoding.UTF8.GetBytes(Convert.ToBase64String(File.ReadAllBytes(Path.Combine(Globals.AppPath, "test.png")))))
                .Concat(Encoding.UTF8.GetBytes(TCPPacket.SEP_STRING))
                .Concat(Encoding.UTF8.GetBytes(Convert.ToBase64String(File.ReadAllBytes(Path.Combine(Globals.AppPath, "test2.png")))))
                .Concat(TCPPacket.GetBinaryFooter()).ToArray();

            for (int i = 0; i < buffer.Length; i += 1024)
            {
                Console.WriteLine($"{i} sent out of {buffer.Length} bytes.");

                stream.Write(buffer, i, buffer.Length - i <= 1024 ? buffer.Length - i : 1024); //sends bytes to server

                Console.WriteLine($"{buffer.Length - i} bytes remaining.");
            }

            stream.Close();
            client.Close();
        }
    }
}
