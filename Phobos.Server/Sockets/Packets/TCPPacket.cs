using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using MiscUtil.Core.Conversion;
using MiscUtil.Core.IO;

namespace Phobos.Server.Sockets.Packets
{
    public class TCPPacket
    {
        /// <summary>
        /// Placeholder string separator for testing.
        /// </summary>
        public const string SEP_STRING = "JordanZeotni";

        /// <summary>
        /// Gets or sets the string used to parse the byte array parameters.
        /// </summary>
        public string TextHeader { get; set; }

        /// <summary>
        /// Bytes representing a Base64 encoded generic object
        /// </summary>
        public byte[] RawDataBytes = {};

        /// <summary>
        /// Switch/case doesn't support types ;P
        /// </summary>
        public static readonly Dictionary<Type, Func<object, byte[]>> BitConversionDictionary = new Dictionary<Type, Func<object, byte[]>>
        {
            {
                typeof(bool), e => BitConverter.GetBytes((bool)e)
            },
            {
                typeof(char), e => BitConverter.GetBytes((char)e)
            },
            {
                typeof(double), e => BitConverter.GetBytes((double)e)
            },
            {
                typeof(short), e => BitConverter.GetBytes((short)e)
            },
            {
                typeof(int), e => BitConverter.GetBytes((int)e)
            },
            {
                typeof(long), e => BitConverter.GetBytes((long)e)
            },
            {
                typeof(float), e => BitConverter.GetBytes((float)e)
            },
            {
                typeof(ushort), e => BitConverter.GetBytes((ushort)e)
            },
            {
                typeof(uint), e => BitConverter.GetBytes((uint)e)
            },
            {
                typeof(ulong), e => BitConverter.GetBytes((ulong)e)
            },
        };

        /// <summary>
        /// Creates a TCPPacket from received raw packet bytes
        /// </summary>
        /// <param name="rawPacket"></param>
        public TCPPacket(byte[] rawPacket)
        {
            int entireLength = EndianBitConverter.Big.ToInt32(rawPacket, 0);

            int headerLength = EndianBitConverter.Big.ToInt32(rawPacket, 4);

            this.TextHeader = Encoding.UTF8.GetString(rawPacket.Skip(8).Take(headerLength).ToArray());

            this.RawDataBytes = rawPacket.Skip(8 + headerLength).ToArray();
        }

        private TCPPacket() { }

        /// <summary>
        /// Create a packet with just a header.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public static TCPPacket CreatePacket(string header)
        {
            return new TCPPacket { TextHeader = header };
        }

        public static bool ByteEquals(byte[] source, byte[] separator, int index)
        {
            return !separator.Where((t, i) => index + i >= source.Length || source[index + i] != t).Any();
        }
        
        /// <summary>
        /// Binary header used to find the start of the packet. Avoids reading partial, corrupt, or unauthorized packets.
        /// </summary>
        /// <returns></returns>
        public static byte[] GetBinaryHeader()
        {
            List<byte> headerList = new List<byte>();

            headerList.AddRange(Encoding.UTF8.GetBytes("PHOBOS"));

            return headerList.ToArray();
        }

        /// <summary>
        /// Binary header used to find the end of the packet. Avoids reading partial, corrupt, or unauthorized packets.
        /// </summary>
        /// <returns></returns>
        public static byte[] GetBinaryFooter()
        {
            List<byte> headerList = new List<byte>();

            headerList.AddRange(Encoding.UTF8.GetBytes("PHOBOS"));

            return headerList.AsEnumerable().Reverse().ToArray();
        }
    }
}
