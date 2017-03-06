using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoTracker.Interfaces
{
    public interface IMongoSet<T> : IMongoSet
    {
        ICollection<T> Loaded { get; }
        void Add(T item);

        void Clear();

        bool Contains(T item);

        void CopyTo(T[] array, int arrayIndex);

        int Count { get; }

        bool IsReadOnly { get; }

        bool Remove(T item);
    }
}
