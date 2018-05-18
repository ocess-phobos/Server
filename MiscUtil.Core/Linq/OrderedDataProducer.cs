using System;
using System.Collections.Generic;
using MiscUtil.Core.Extensions;
using Action = MiscUtil.Core.DotNet20.Action;

namespace MiscUtil.Core.Linq
{
    /// <summary>
    /// A DataProducer with ordering capabilities
    /// </summary><remarks>Note that this may cause data to be buffered</remarks>
    /// <typeparam name="T"></typeparam>
    internal class OrderedDataProducer<T> : IOrderedDataProducer<T>
    {
        private bool dataHasEnded;
        private readonly IDataProducer<T> baseProducer;
        private readonly IComparer<T> comparer;
        private List<T> buffer;

        public IDataProducer<T> BaseProducer
        {
            get { return this.baseProducer; }
        }

        public IComparer<T> Comparer
        {
            get { return this.comparer; }
        }

        public event Action<T> DataProduced;
        public event Action EndOfData;

        /// <summary>
        /// Create a new OrderedDataProducer
        /// </summary>
        /// <param name="baseProducer">The base source which will supply data</param>
        /// <param name="comparer">The comparer to use when sorting the data (once complete)</param>
        public OrderedDataProducer(
            IDataProducer<T> baseProducer,
            IComparer<T> comparer)
        {
            baseProducer.ThrowIfNull("baseProducer");
            
            this.baseProducer = baseProducer;
            this.comparer = comparer ?? Comparer<T>.Default;

            baseProducer.DataProduced += new Action<T>(this.OriginalDataProduced);
            baseProducer.EndOfData += new Action(this.EndOfOriginalData);
        }


        void OriginalDataProduced(T item)
        {
            if (this.dataHasEnded)
            {
                throw new InvalidOperationException("EndOfData already occurred");
            }
            if (this.DataProduced != null)
            { // only get excited if somebody is listening
                if (this.buffer == null) this.buffer = new List<T>();
                this.buffer.Add(item);
            }
        }

        void EndOfOriginalData()
        {
            if (this.dataHasEnded)
            {
                throw new InvalidOperationException("EndOfData already occurred");
            }
            this.dataHasEnded = true;
            // only do the sort if somebody is still listening
            if (this.DataProduced != null && this.buffer != null)
            {
                this.buffer.Sort(this.Comparer);
                foreach (T item in this.buffer)
                {
                    this.OnDataProduced(item);
                }
            }
            this.buffer = null;
            this.OnEndOfData();
        }

        void OnEndOfData()
        {
            if (this.EndOfData != null) this.EndOfData();
        }

        void OnDataProduced(T item)
        {
            if (this.DataProduced != null) this.DataProduced(item);
        }
    }
}