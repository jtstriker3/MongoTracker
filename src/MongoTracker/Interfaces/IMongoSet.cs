using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoTracker.Interfaces
{
    public interface IMongoSet : IQueryable
    {
        IMongoCollection<T> GetCollection<T>();
        Boolean FlaggedForDeletion { get; }
        void AddToLoaded(object item);
    }
}
