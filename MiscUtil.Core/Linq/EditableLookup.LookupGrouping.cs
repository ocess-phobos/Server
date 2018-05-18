using System.Collections.Generic;
using MiscUtil.Core.DotNet20;

namespace MiscUtil.Core.Linq
{
    partial class EditableLookup<TKey, TElement>
	{
        internal sealed class LookupGrouping : IGrouping<TKey, TElement>
        {
            private readonly TKey key;
            private List<TElement> items = new List<TElement>();
            public TKey Key { get { return this.key; } }
            public LookupGrouping(TKey key)
            {
                this.key = key;
            }
            public int Count
            {
                get { return this.items.Count; }
            }
            public void Add(TElement item)
            {
                this.items.Add(item);
            }
            public bool Contains(TElement item)
            {
                return this.items.Contains(item);
            }
            public bool Remove(TElement item)
            {
                return this.items.Remove(item);
            }
            public void TrimExcess()
            {
                this.items.TrimExcess();
            }

            public IEnumerator<TElement> GetEnumerator()
            {
                return this.items.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
	}
}
