using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoTracker.Utility.Extensions;
using MongoTracker.Interfaces;
using System.Reflection;

namespace MongoTracker
{
    public class MongoSet<T> : IEnumerable<T>, IQueryable<T>, ICollection<T>, IMongoSet<T>, IMongoSet
        where T : IObjectId
    {
        protected MongoContext Context { get; set; }
        protected internal IMongoCollection<T> CollectionGeneric { get; set; }
        protected IMongoDatabase Database { get; set; }
        public ICollection<T> Loaded { get; private set; }
        public Boolean FlaggedForDeletion { get; private set; }
        protected InterceptingProvider TrackerProvider { get; set; }
        protected IQueryable<T> Query { get; set; }

        public MongoSet(MongoContext context, String name)
        {
            Context = context;
            Database = context.Database;
            CollectionGeneric = Database.GetCollection<T>(name);
            //Queryable = CollectionGeneric.AsQueryable();
            Query = InterceptingProvider.Intercept(this.CollectionGeneric.AsQueryable(), this.Context);
            Loaded = new List<T>();
        }

        #region IQueryable/IEnumerable Implementation

        public IEnumerator<T> GetEnumerator()
        {
            if (!FlaggedForDeletion)
            {
                var items = this.CollectionGeneric.AsQueryable().ToList();

                foreach (var item in items)
                    this.Context.AddObjectToTracking(item);
            }
            //Need to figure out how this is goint to work with the tracker provider
            return Loaded.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get
            {
                return Query.Expression;
            }
        }

        public IQueryProvider Provider
        {
            get
            {
                return Query.Provider;
            }
        }

        #endregion

        public void Add(T item)
        {
            if (this.Context.Trackers.ContainsKey(item.Id))
                throw new Exception("This Object is Already Be Tracked!");

            var tracker = new Tracker<T>(item);
            tracker.State = State.Added;
            Context.Trackers.Add(item.Id, tracker);
            this.Loaded.Add(item);
        }

        public void Clear()
        {
            Loaded.Clear();
            FlaggedForDeletion = true;
        }

        public bool Contains(T item)
        {
            if (this.Loaded.Contains(item))
                return true;
            else if (this.CollectionGeneric.AsQueryable().Contains(item))
                return true;

            return false;

        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in this.CollectionGeneric.AsQueryable())
            {
                array[arrayIndex] = item;
                arrayIndex++;
            }
        }

        public int Count
        {
            get
            {
                return this.CollectionGeneric.AsQueryable().Count();
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            var key = item.Id;
            if (this.Context.Trackers.ContainsKey(key))
            {
                var tracker = this.Context.Trackers[key];
                tracker.State = State.Deleted;
                return true;
            }

            return false;
        }

        public IMongoCollection<T1> GetCollection<T1>()
        {
            return CollectionGeneric as IMongoCollection<T1>;
        }

        public void AddToLoaded(object item)
        {
            if (item != null)
                if (typeof(T).GetTypeInfo().IsAssignableFrom(item.GetType()))
                    Loaded.Add((T)item);
                else
                    throw new ArgumentException($"Item does not match {typeof(T).FullName}!");
            else
                throw new ArgumentException($"Item cannot be null!");
        }
    }
}
