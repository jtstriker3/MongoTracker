using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoTracker.Test.Model
{
    public class Parent
    {
        public ObjectId Id { get; set; }
        public String Name { get; set; }
    }

    public enum ParentType
    {
        Mom,
        Dad
    }
}
