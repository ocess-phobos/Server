using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

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
        public List<byte[]> DataBytesList = new List<byte[]>();

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
            File.WriteAllBytes(Path.Combine(Globals.AppPath, "test.bin"), rawPacket);

            int headerLength = BitConverter.ToInt32(rawPacket, GetBinaryHeader().Length);

            List<byte> postBinaryHeaderBytes = rawPacket.Skip(GetBinaryHeader().Length + 4).ToList();

            this.TextHeader = Encoding.UTF8.GetString(postBinaryHeaderBytes.Take(headerLength).ToArray());
            
            Debug.WriteLine($"[DEBUG]: {rawPacket[GetBinaryHeader().Length + 1]} {rawPacket[GetBinaryHeader().Length + 2]} {rawPacket[GetBinaryHeader().Length + 3]} {rawPacket[GetBinaryHeader().Length + 4]} Header length: {headerLength}, {this.TextHeader}");

            if (Encoding.UTF8.GetString(postBinaryHeaderBytes.Skip(headerLength).ToArray()).Contains(SEP_STRING))
            {
                string[] fullBase64Data = Encoding.UTF8.GetString(postBinaryHeaderBytes.Skip(headerLength).ToArray()).Split(new[] {SEP_STRING}, StringSplitOptions.None);

                if (fullBase64Data.Length == 0)
                {
                    return;
                }

                this.DataBytesList = fullBase64Data.Select(Encoding.UTF8.GetBytes).ToList();
            }
            else
            {
                this.DataBytesList.Add(postBinaryHeaderBytes.Skip(headerLength).ToArray());
            }
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

        /// <summary>
        /// Create a packet with raw byte array parameters
        /// </summary>
        /// <param name="header"></param>
        /// <param name="byteArrays"></param>
        /// <returns></returns>
        public static TCPPacket CreatePacket(string header, params byte[][] byteArrays)
        {
            TCPPacket packet = new TCPPacket { TextHeader = header };

            packet.DataBytesList.AddRange(byteArrays);

            return packet;
        }

        /// <summary>
        /// Create a packet with UTF8 strings
        /// </summary>
        /// <param name="header"></param>
        /// <param name="strings"></param>
        /// <returns></returns>
        public static TCPPacket CreatePacket(string header, params string[] strings)
        {
            TCPPacket packet = new TCPPacket { TextHeader = header };

            foreach (string s in strings)
            {
                packet.DataBytesList.Add(Encoding.UTF8.GetBytes(s));
            }

            return packet;
        }

        /// <summary>
        /// Converts object parameters to byte arrays using BitConverter. As such, if the parameters are not convertible with BitConverter, this method will throw an exception.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static TCPPacket CreatePacket(string header, params object[] parameters)
        {
            TCPPacket packet = new TCPPacket { TextHeader = header };

            try
            {
                foreach (object parameter in parameters)
                {
                    packet.DataBytesList.Add(BitConversionDictionary[parameter.GetType()].Invoke(parameter));
                }
            }
            catch
            {
                throw new ArgumentException("One or more parameters are not convertible with BitConverter.");
            }

            return packet;
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

            headerList.AddRange(BitConverter.GetBytes('P'));
            headerList.AddRange(BitConverter.GetBytes('H'));
            headerList.AddRange(BitConverter.GetBytes('O'));
            headerList.AddRange(BitConverter.GetBytes('B'));
            headerList.AddRange(BitConverter.GetBytes('O'));
            headerList.AddRange(BitConverter.GetBytes('S'));

            return headerList.ToArray();
        }

        /// <summary>
        /// Binary header used to find the end of the packet. Avoids reading partial, corrupt, or unauthorized packets.
        /// </summary>
        /// <returns></returns>
        public static byte[] GetBinaryFooter()
        {
            List<byte> headerList = new List<byte>();

            headerList.AddRange(BitConverter.GetBytes('P'));
            headerList.AddRange(BitConverter.GetBytes('H'));
            headerList.AddRange(BitConverter.GetBytes('O'));
            headerList.AddRange(BitConverter.GetBytes('B'));
            headerList.AddRange(BitConverter.GetBytes('O'));
            headerList.AddRange(BitConverter.GetBytes('S'));

            return headerList.AsEnumerable().Reverse().ToArray();
        }
    }
}
