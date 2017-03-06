using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoTracker
{
    public enum State
    {
        UnChanged,
        Modified,
        Added,
        Deleted
    }
}
