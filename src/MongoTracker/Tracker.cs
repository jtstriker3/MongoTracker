using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoTracker.Utility.Extensions;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using MongoTracker.Interfaces;

namespace MongoTracker
{
    public abstract class Tracker : IComparable<Tracker>, IComparable
    {
        public abstract IEnumerable<Object> GetKey();
        protected abstract Boolean HasChanged();
        public abstract State State { get; set; }
        public IObjectId LiveObj { get; set; }
        public Type Type { get; private set; }

        public Tracker(Type type)
        {
            Type = type;
        }

        public int CompareTo(Tracker other)
        {
            return this.CompareTo((Object)other);
        }

        public int CompareTo(object obj)
        {
            var compareTo = (Tracker)obj;

            var compareToContainsType = compareTo.Type.GetTypes().Contains(this.Type, new Utility.Comparers.TypeCompare());
            var thisHasType = this.Type.GetTypes().Contains(compareTo.Type);

            if (compareToContainsType && thisHasType)
                throw new NotImplementedException("Have Not Implemented Cyclical Relations");
            else if (thisHasType)
                return 1;
            else if (compareToContainsType)
                return -1;

            return 0;
            //Check of compare to type has subtype
        }
    }

    class Tracker<T> : Tracker
        where T : IObjectId
    {
        protected String Hash { get; set; }
        public T Live { get; set; }
        private Boolean _stateSetManually = false;
        private State _state;
        public IMongoCollection<T> CollectionGeneric { get; set; }

        public Tracker(T obj)
            : base(obj?.GetType())
        {
            if (obj == null)
                throw new ArgumentException("obj cannot be null", "T: obj");
            Live = obj;
            LiveObj = obj;
            Hash = obj.Hash();
        }

        public override IEnumerable<Object> GetKey()
        {
            return this.Live.GetIds();
        }

        protected override Boolean HasChanged()
        {
            return Live.Hash() != Hash;
        }

        public override State State
        {
            get
            {
                if (_stateSetManually)
                    return _state;
                else
                {
                    if (Live.Hash() != Hash)
                        return State.Modified;
                    else
                        return State.UnChanged;
                }
            }
            set
            {
                _state = value;
                _stateSetManually = true;
            }

        }
    }
}
