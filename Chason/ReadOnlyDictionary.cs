namespace Chason
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> dictionary;

        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
        {
            var readOnly = dictionary as ReadOnlyDictionary<TKey, TValue>;
            this.dictionary = readOnly != null ? readOnly.dictionary : dictionary;
            this.Keys = dictionary.Keys;
            this.Values = dictionary.Values;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException("Dictionary is read-only");
        }

        public void Clear()
        {
            throw new NotSupportedException("Dictionary is read-only");
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotSupportedException("Dictionary is read-only");
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException("Dictionary is read-only");
        }

        public int Count { get; private set; }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public void Add(TKey key, TValue value)
        {
            throw new NotSupportedException("Dictionary is read-only");
        }

        public bool ContainsKey(TKey key)
        {
            return this.dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            throw new NotSupportedException("Dictionary is read-only");
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                return this.dictionary[key];
            }
            set
            {
                throw new NotSupportedException("Dictionary is read-only");
            }
        }

        public ICollection<TKey> Keys { get; private set; }

        public ICollection<TValue> Values { get; private set; }
    }
}