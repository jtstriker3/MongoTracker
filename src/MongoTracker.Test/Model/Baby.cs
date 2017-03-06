using MongoDB.Bson;
using MongoTracker.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoTracker.Test.Model
{
    public class Baby : IObjectId
    {
        public ObjectId Id { get; set; }
        public String Name { get; set; }
        public DateTime BirthDay { get; set; }

        public List<Toy> Toys { get; set; }
    }
}
