using MongoTracker.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MongoTracker
{
    public class InterceptedQuery<T> : IOrderedQueryable<T>
    {
        private Expression _expression;
        private InterceptingProvider _provider;
        private MongoContext _context;

        public InterceptedQuery(
           InterceptingProvider provider,
           Expression expression,
            MongoContext context)
        {
            this._provider = provider;
            this._expression = expression;
            this._context = context;
        }
        public IEnumerator<T> GetEnumerator()
        {

            var result = new List<T>();
            var enumerator = this._provider.ExecuteQuery<T>(this._expression);
            var isIObjectId = typeof(IObjectId).GetTypeInfo().IsAssignableFrom(typeof(T));

            while (enumerator.MoveNext())
            {
                if (isIObjectId)
                {
                    var castObject = enumerator.Current as IObjectId;
                    var trackedItem = this._context.AddObjectToTracking(castObject);
                    if (trackedItem != null)
                        result.Add((T)trackedItem);
                    else
                        result.Add(enumerator.Current);
                }
                else
                    result.Add(enumerator.Current);
            }
            //MongoDb Does not support
            //enumerator.Reset();

            return result.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            var result = new List<T>();
            var enumerator = this._provider.ExecuteQuery<T>(this._expression);
            var isIObjectId = typeof(IObjectId).GetTypeInfo().IsAssignableFrom(typeof(T));

            while (enumerator.MoveNext())
            {
                if (isIObjectId)
                {
                    var castObject = enumerator.Current as IObjectId;
                    var trackedItem = this._context.AddObjectToTracking(castObject);
                    if (trackedItem != null)
                        result.Add((T)trackedItem);
                    else
                        result.Add(enumerator.Current);
                }
                else
                    result.Add(enumerator.Current);
            }
            //MongoDb Does not support
            //enumerator.Reset();
            return result.GetEnumerator();
        }
        public Type ElementType
        {
            get { return typeof(T); }
        }
        public Expression Expression
        {
            get { return this._expression; }
        }
        public IQueryProvider Provider
        {
            get { return this._provider; }
        }
    }
}
