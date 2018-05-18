using System;
using System.IO;

namespace MiscUtil.Core.Compression.Vcdiff
{
	/// <summary>
	/// Cache used for encoding/decoding addresses.
	/// </summary>
	internal sealed class AddressCache
	{
		const byte SelfMode = 0;
		const byte HereMode = 1;

		int nearSize;
		int sameSize;
		int[] near;
		int nextNearSlot;
		int[] same;

		Stream addressStream;

		internal AddressCache(int nearSize, int sameSize)
		{
			this.nearSize = nearSize;
			this.sameSize = sameSize;
			this.near = new int[nearSize];
			this.same = new int[sameSize*256];
		}

		internal void Reset(byte[] addresses)
		{
			this.nextNearSlot = 0;
			Array.Clear(this.near, 0, this.near.Length);
			Array.Clear(this.same, 0, this.same.Length);

			this.addressStream = new MemoryStream(addresses, false);
		}

		internal int DecodeAddress (int here, byte mode)
		{
			int ret;
			if (mode==SelfMode)
			{
				ret = IOHelper.ReadBigEndian7BitEncodedInt(this.addressStream);
			}
			else if (mode==HereMode)
			{
				ret = here - IOHelper.ReadBigEndian7BitEncodedInt(this.addressStream);
			}
			else if (mode-2 < this.nearSize) // Near cache
			{
				ret = this.near[mode-2] + IOHelper.ReadBigEndian7BitEncodedInt(this.addressStream);
			}
			else // Same cache
			{
				int m = mode-(2+this.nearSize);
				ret = this.same[(m*256)+IOHelper.CheckedReadByte(this.addressStream)];
			}

			this.Update (ret);
			return ret;
		}

		void Update (int address)
		{
			if (this.nearSize > 0)
			{
				this.near[this.nextNearSlot] = address;
				this.nextNearSlot=(this.nextNearSlot+1)%this.nearSize;
			}
			if (this.sameSize > 0)
			{
				this.same[address%(this.sameSize*256)] = address;
			}
		}
	}
}
