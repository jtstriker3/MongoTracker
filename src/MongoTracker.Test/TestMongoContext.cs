using MongoTracker.Test.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoTracker.Test
{
    public class TestMongoContext : MongoContext
    {
        public TestMongoContext(string connectionString, bool useSsl = true) : base(connectionString, useSsl) { }
        public MongoSet<Baby> Babies { get; set; }
    }
}
