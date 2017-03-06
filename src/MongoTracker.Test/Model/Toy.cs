using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoTracker.Test.Model
{
    public class Toy
    {
        public String Name { get; set; }
        public ToyGender ToyGender { get; set; }
    }

    public enum ToyGender
    {
        Male,
        Female,
        Both
    }
}
