using System.Collections.Generic;

namespace MiscUtil.Core.Linq
{
    /// <summary>
    /// Ordered variant of IDataProducer; note that generally
    /// this will force data to be buffered until the sequence
    /// is complete.
    /// </summary>
    /// <seealso cref="IDataProducer{T}"/>
    public interface IOrderedDataProducer<T> : IDataProducer<T>
    {
        /// <summary>
        /// The unlerlying producer that can push data
        /// </summary>
        IDataProducer<T> BaseProducer { get; }
        /// <summary>
        /// The comparer used to order the sequence (once complete)
        /// </summary>
        IComparer<T> Comparer { get; }
    }
}
