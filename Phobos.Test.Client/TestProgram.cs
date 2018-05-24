using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using MiscUtil.Core.Conversion;
using Phobos.Server.Sockets.Packets;

namespace Phobos.Test.Client
{
    public static class TestProgram
    {
        public static void Main(string[] args)
        {
            TcpClient client = new TcpClient("localhost", 11000);
            NetworkStream stream = client.GetStream();

            byte[] header = EndianBitConverter.Big.GetBytes(Encoding.UTF8.GetBytes("flight").Length)
                .Concat(Encoding.UTF8.GetBytes("flight"))
                .ToArray();

            header = EndianBitConverter.Big.GetBytes(header.Length + 4)
                .Concat(header)
                .ToArray();

            stream.Write(header, 0, header.Length);

            byte[] buffer = EndianBitConverter.Big.GetBytes(Encoding.UTF8.GetBytes("testimage").Length)
                .Concat(Encoding.UTF8.GetBytes("testimage"))
                .Concat(EndianBitConverter.Big.GetBytes(File.ReadAllBytes(Path.Combine(Globals.AppPath, "test.png")).Length))
                .Concat(File.ReadAllBytes(Path.Combine(Globals.AppPath, "test.png")))
                .Concat(EndianBitConverter.Big.GetBytes(File.ReadAllBytes(Path.Combine(Globals.AppPath, "test2.png")).Length))
                .Concat(File.ReadAllBytes(Path.Combine(Globals.AppPath, "test2.png")))
                .ToArray();

            buffer = EndianBitConverter.Big.GetBytes(buffer.Length + 4)
                .Concat(buffer)
                .ToArray();

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
