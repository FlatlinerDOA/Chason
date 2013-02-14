//--------------------------------------------------------------------------------------------------
// <copyright file="ReadOnlyHashSet.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Wraps an <see cref="ISet{T}"/> implementation to ensure it cannot be modified
    /// </summary>
    /// <typeparam name="T">The type in the set</typeparam>
    public sealed class ReadOnlyHashSet<T> : ISet<T>
    {
        private readonly ISet<T> set;

        public ReadOnlyHashSet(ISet<T> set)
        {
            var readOnly = set as ReadOnlyHashSet<T>;
            this.set = readOnly != null ? readOnly.set : set;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return this.set.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return this.set.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return this.set.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return this.set.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return this.set.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return this.set.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            return this.set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.set.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public int Count
        {
            get
            {
                return this.set.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }
    }
}