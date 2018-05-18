using System;

namespace MiscUtil.Core
{
    /// <summary>
    /// Type of buffer returned by CachingBufferManager.
    /// </summary>
    class CachedBuffer : IBuffer
    {
        readonly byte[] data;
        volatile bool available;
        readonly bool clearOnDispose;

        internal CachedBuffer(int size, bool clearOnDispose)
        {
            this.data = new byte[size];
            this.clearOnDispose = clearOnDispose;
        }

        internal bool Available
        {
            get { return this.available; }
            set { this.available = value; }
        }

        public byte[] Bytes
        {
            get { return this.data; }
        }

        public void Dispose()
        {
            if (this.clearOnDispose)
            {
                Array.Clear(this.data, 0, this.data.Length);
            }
            this.available = true;
        }
    }
}
