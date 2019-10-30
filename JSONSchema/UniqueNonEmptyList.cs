using System;
using System.Collections.Generic;
using System.Text;

namespace JSONSchema2POCO
{
    public class UniqueNonEmptyList<T> : List<T>
    {
        private readonly HashSet<T> ValueSet = new HashSet<T>();

        protected UniqueNonEmptyList()
        {
            throw new InvalidOperationException();
        }

        protected UniqueNonEmptyList(int capacity)
        {
            throw new InvalidOperationException();
        }

        public UniqueNonEmptyList(T t)
        {
            this.Add(t);
        }

        public new void Add(T item)
        {
            if (ValueSet.Contains(item))
            {
                throw new ArgumentException(item + " is already defined");
            }
            ValueSet.Add(item);
            base.Add(item);
        }

        public UniqueNonEmptyList(ICollection<T> collection)
        {
            if (collection.Count < 1)
            {
                throw new ArgumentException();
            }
            foreach (T t in collection)
            {                
                this.Add(t);
            }
        }

        public new bool Remove(T item)
        {
            if(this.Count == 1 && this.Contains(item))
            {
                return false;
            }
            return base.Remove(item);
        }
    }
}

