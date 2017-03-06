using MongoTracker.Test.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoTracker.Test
{
    public class TestMongoContext : MongoContext
    {
        public TestMongoContext(string connectionString) : base(connectionString) { }
        public MongoSet<Baby> Babies { get; set; }
    }
}
