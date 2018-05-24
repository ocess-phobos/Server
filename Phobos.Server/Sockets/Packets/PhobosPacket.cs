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
    public class PhobosPacket
    {
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
                typeof(bool), e => EndianBitConverter.Big.GetBytes((bool)e)
            },
            {
                typeof(char), e => EndianBitConverter.Big.GetBytes((char)e)
            },
            {
                typeof(double), e => EndianBitConverter.Little.GetBytes((double)e)
            },
            {
                typeof(short), e => EndianBitConverter.Big.GetBytes((short)e)
            },
            {
                typeof(int), e => EndianBitConverter.Big.GetBytes((int)e)
            },
            {
                typeof(long), e => EndianBitConverter.Big.GetBytes((long)e)
            },
            {
                typeof(float), e => EndianBitConverter.Little.GetBytes((float)e)
            },
            {
                typeof(ushort), e => EndianBitConverter.Big.GetBytes((ushort)e)
            },
            {
                typeof(uint), e => EndianBitConverter.Big.GetBytes((uint)e)
            },
            {
                typeof(ulong), e => EndianBitConverter.Big.GetBytes((ulong)e)
            },
        };

        /// <summary>
        /// Creates a TCPPacket from received raw packet bytes
        /// </summary>
        /// <param name="rawPacket"></param>
        public PhobosPacket(byte[] rawPacket)
        {
#if DEBUG
            // Write to file to debug packet receive errors
            File.WriteAllBytes(Path.Combine(Globals.AppPath, $"packet-{DateTime.Now.ToFileTime()}"), rawPacket);
#endif

            int entireLength = EndianBitConverter.Big.ToInt32(rawPacket, 0);

            int headerLength = EndianBitConverter.Big.ToInt32(rawPacket, 4);

            this.TextHeader = Encoding.UTF8.GetString(rawPacket.Skip(8).Take(headerLength).ToArray());

            Debug.WriteLine(this.TextHeader);

            if (rawPacket.Length == 8 + headerLength)
            {
                return;
            }

            this.RawDataBytes = rawPacket.Skip(8 + headerLength).ToArray();
        }

        private PhobosPacket() { }

        /// <summary>
        /// Create a packet with just a header.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public static PhobosPacket CreatePacket(string header)
        {
            return new PhobosPacket { TextHeader = header };
        }

        /// <summary>
        /// Create a TCP packet using BitConverter. If the type of any of the object(s) is not a convertible type, throws <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static PhobosPacket CreatePacketWithBitConverter(string header, params object[] objects)
        {
            PhobosPacket phobosPacket = new PhobosPacket
            {
                TextHeader = header,
                RawDataBytes = new byte[] {}
            };

            foreach (object o in objects)
            {
                if (!BitConversionDictionary.ContainsKey(o.GetType()))
                {
                    throw new ArgumentException($"Cannot convert type {o.GetType()}.");
                }

                phobosPacket.RawDataBytes = phobosPacket.RawDataBytes.Concat(BitConversionDictionary[o.GetType()].Invoke(o)).ToArray();
            }

            return phobosPacket;
        }
    }
}
