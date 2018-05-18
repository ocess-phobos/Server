using System;
using System.Collections;
using System.Collections.Generic;
using MiscUtil.Core.Collections.Extensions;
using MiscUtil.Core.Extensions;

namespace MiscUtil.Core.Collections
{
    /// <summary>
    /// Iterates over a range. Despite its name, this implements IEnumerable{T} rather than
    /// IEnumerator{T} - it just sounds better, frankly.
    /// </summary>
    public class RangeIterator<T> : IEnumerable<T>
    {
        readonly Range<T> range;
        /// <summary>
        /// Returns the range this object iterates over
        /// </summary>
        public Range<T> Range
        {
            get { return this.range; }
        }

        readonly DotNet20.Func<T, T> step;
        /// <summary>
        /// Returns the step function used for this range
        /// </summary>
        public DotNet20.Func<T, T> Step
        {
            get { return this.step; }
        }

        readonly bool ascending;
        /// <summary>
        /// Returns whether or not this iterator works up from the start point (ascending)
        /// or down from the end point (descending)
        /// </summary>
        public bool Ascending
        {
            get { return this.@ascending; }
        }

        /// <summary>
        /// Creates an ascending iterator over the given range with the given step function
        /// </summary>
        public RangeIterator(Range<T> range, DotNet20.Func<T, T> step)
            : this(range, step, true)
        {
        }

        /// <summary>
        /// Creates an iterator over the given range with the given step function,
        /// with the specified direction.
        /// </summary>
        public RangeIterator(Range<T> range, DotNet20.Func<T, T> step, bool ascending)
        {
            step.ThrowIfNull("step");

            if ((ascending && range.Comparer.Compare(range.Start, step(range.Start)) >= 0) ||
                (!ascending && range.Comparer.Compare(range.End, step(range.End)) <= 0))
            {
                throw new ArgumentException("step does nothing, or progresses the wrong way");
            }
            this.ascending = ascending;
            this.range = range;
            this.step = step;
        }

        /// <summary>
        /// Returns an IEnumerator{T} running over the range.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            // A descending range effectively has the start and end points (and inclusions)
            // reversed, and a reverse comparer.
            bool includesStart = this.@ascending ? this.range.IncludesStart : this.range.IncludesEnd;
            bool includesEnd = this.@ascending ? this.range.IncludesEnd : this.range.IncludesStart;
            T start = this.@ascending ? this.range.Start : this.range.End;
            T end = this.@ascending ? this.range.End : this.range.Start;
            IComparer<T> comparer = this.@ascending ? this.range.Comparer : this.range.Comparer.Reverse();

            // Now we can use our local version of the range variables to iterate

            T value = start;

            if (includesStart)
            {
                // Deal with possibility that start point = end point
                if (includesEnd || comparer.Compare(value, end) < 0)
                {
                    yield return value;
                }
            }
            value = this.step(value);

            while (comparer.Compare(value, end) < 0)
            {
                yield return value;
                value = this.step(value);
            }

            // We've already performed a step, therefore we can't
            // still be at the start point
            if (includesEnd && comparer.Compare(value, end) == 0)
            {
                yield return value;
            }
        }

        /// <summary>
        /// Returns an IEnumerator running over the range.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
