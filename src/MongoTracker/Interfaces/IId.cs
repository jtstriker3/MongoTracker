using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoTracker.Interfaces
{
    public interface IObjectId
    {
        ObjectId Id { get; }
    }
}
