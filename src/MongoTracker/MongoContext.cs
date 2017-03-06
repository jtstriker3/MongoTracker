using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Linq.Expressions;
using MongoTracker.Utility.Extensions;
using MongoTracker.Interfaces;
using MongoDB.Bson;
using System.Reflection;

namespace MongoTracker
{
    public class MongoContext : IDisposable
    {
        public MongoUrl ConnectionString { get; set; }
        public IMongoDatabase Database { get; set; }
        protected IDictionary<Type, IMongoSet> MongoSets { get; set; }
        protected internal IDictionary<ObjectId, Tracker> Trackers { get; set; }
        protected internal Dictionary<Type, Type> TrackableTypes { get; set; }

        public MongoContext(String connectionString)
        {
            //connectionString = connectionString ?? System.Configuration.ConfigurationManager.ConnectionStrings[this.GetType().Name].ConnectionString;
            ConnectionString = MongoUrl.Create(connectionString);
            this.Database = this.GetDataConnection();
            this.MongoSets = new Dictionary<Type, IMongoSet>();
            this.Trackers = new Dictionary<ObjectId, Tracker>();
            this.TrackableTypes = new Dictionary<Type, Type>();
            this.InitMongoSets();
        }

        public MongoContext()
            : this(null)
        {

        }

        protected void InitMongoSets()
        {
            //This list Should Be Cached in the Future
            var properties = this.GetType().GetTypeInfo().GetProperties().Where(prop => prop.PropertyType.GetTypeInfo().IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(MongoSet<>));

            foreach (var prop in properties)
            {
                var genericType = prop.PropertyType.GetTypeInfo().GetGenericArguments().First();
                var newMongoSet = (IMongoSet)prop.PropertyType.CreateInstance(this, prop.Name);
                prop.SetValue(this, newMongoSet);
                this.MongoSets.Add(newMongoSet.ElementType, newMongoSet);

                this.TrackableTypes.Add(genericType, genericType);
            }
        }

        protected IMongoDatabase GetDataConnection()
        {
            MongoClient client = new MongoClient(this.ConnectionString.ToString());
            return client.GetDatabase(this.ConnectionString.DatabaseName);
        }

        public IMongoSet Set(Type type)
        {
            return MongoSets[type];
        }

        public IMongoSet<T> Set<T>()
            where T : IObjectId
        {
            return (IMongoSet<T>)this.Set(typeof(T));
        }

        public async Task<int> SaveChanges()
        {
            var count = 0;
            var allInserts = Trackers.Where(tracker => tracker.Value.State == State.Added).Select(tracker => tracker.Value).ToList();

            var groupedBulkInserts = allInserts.GroupBy(b => b.Type);
            foreach (var group in groupedBulkInserts)
            {
                var method = typeof(MongoContext).GetTypeInfo().GetMethod(nameof(InsertBatch), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method = method.MakeGenericMethod(group.Key);
                var task = (Task)method.Invoke(this, new[] { group });
                await task;
                count += group.Count();
            }


            var saveChangesMethod = typeof(MongoContext).GetTypeInfo().GetMethod(nameof(SaveChangesGeneric), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var trackerKeyValuePair in Trackers)
            {
                var genericSaveChangesMethod = saveChangesMethod.MakeGenericMethod(trackerKeyValuePair.Value.Type);
                var task = (Task<bool>)genericSaveChangesMethod.Invoke(this, new[] { trackerKeyValuePair.Value });
                if (await task)
                    count++;
            }

            return count;
        }

        private async Task<bool> SaveChangesGeneric<T>(Tracker tracker)
            where T : IObjectId
        {
            switch (tracker.State)
            {
                case State.Modified:
                    var mongoSet = this.Set(tracker.Type);
                    if (!mongoSet.FlaggedForDeletion)
                    {
                        var filter = Builders<T>.Filter.Eq(t => t.Id, tracker.LiveObj.Id);
                        await mongoSet.GetCollection<T>().ReplaceOneAsync(filter, (T)tracker.LiveObj);
                    }
                    return true;
                case State.Deleted:
                    //var ids = tracker.LiveObj.GetIds();
                    //var query = new QueryDocument();
                    //figure out way to delete documents without knowing id
                    //tracker.Collection.Remove();
                    mongoSet = this.Set(tracker.Type);
                    if (!mongoSet.FlaggedForDeletion)
                    {
                        var filter = Builders<T>.Filter.Eq(t => t.Id, tracker.LiveObj.Id);
                        await mongoSet.GetCollection<T>().DeleteOneAsync(filter);
                    }
                    return true;
            }

            return false;
        }

        private async Task InsertBatch<T>(IEnumerable<Tracker> allInserts)
            where T : IObjectId
        {
            await Set<T>().GetCollection<T>().InsertManyAsync(allInserts.Select(item => item.LiveObj).Cast<T>());
        }

        public T AddObjectToTracking<T>(T item)
            where T : IObjectId
        {

            if (item != null)
            {
                var type = item.GetType();
                if (TrackableTypes.ContainsKey(type))
                {
                    var key = item.GetHashCode();

                    if (Trackers.ContainsKey(item.Id))
                        return (T)Trackers[item.Id].LiveObj;
                    else
                    {
                        this.Set(type).AddToLoaded(item);
                        var currentTracker = new Tracker<T>(item);
                        this.Trackers.Add(item.Id, currentTracker);
                        return item;
                    }

                }
            }
            return default(T);
        }

        public void Dispose()
        {
           
        }
    }
}
