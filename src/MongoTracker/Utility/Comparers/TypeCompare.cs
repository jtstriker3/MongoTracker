using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MongoTracker.Utility.Comparers
{
    public class TypeCompare : IEqualityComparer<Type>
    {

        public bool Equals(Type x, Type y)
        {
            return x.GetTypeInfo().IsAssignableFrom(y) || y.GetTypeInfo().IsAssignableFrom(x);
        }

        public int GetHashCode(Type obj)
        {
            return obj.GetHashCode();
        }
    }
}
