using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using MongoTracker.Utility.Helpers;
using MongoTracker.Interfaces;

namespace MongoTracker
{
    //public class TrackerProvider<T> : IQueryProvider
    //{
    //    protected MongoCollection<T> Collection { get; set; }
    //    protected internal IQueryable<T> Query { get; set; }
    //    protected MongoContext Context { get; set; }

    //    public TrackerProvider(MongoCollection<T> collection, MongoContext context)
    //    {
    //        Collection = collection;
    //        Query = collection.AsQueryable();
    //        Context = context;

    //    }

    //    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
    //    {
    //        return (IQueryable<TElement>)this.CreateQuery(expression);
    //    }

    //    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
    //    {
    //        Query = Query.Provider.CreateQuery<T>(expression);
    //        return Query;
    //    }

    //    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
    //    {
    //        var result = Query.Provider.Execute<TResult>(expression);
    //        //Need To make sure Generic Type is known Type and not a view Model.
    //        if (typeof(IEnumerable<T>).IsAssignableFrom(typeof(TResult)) && !typeof(String).IsAssignableFrom(typeof(TResult)))
    //        {
    //            var list = (IEnumerable)result;

    //            foreach (var item in list)
    //            {
    //                this.Context.AddObjectToTracking(item);
    //            }
    //        }
    //        else if (typeof(TResult) == typeof(T))
    //            this.Context.AddObjectToTracking(result);

    //        return result;
    //    }

    //    public object Execute(System.Linq.Expressions.Expression expression)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class InterceptingProvider : IQueryProvider
    {
        private IQueryProvider _underlyingProvider;
        private Func<Expression, Expression>[] _visitors;
        private MongoContext _context;

        public InterceptingProvider(
            IQueryProvider underlyingQueryProvider,
            MongoContext context,
            params Func<Expression, Expression>[] visitors)
        {
            this._underlyingProvider = underlyingQueryProvider;
            this._visitors = visitors;
            this._context = context;
        }

        public static IQueryable<T> Intercept<T>(
            IQueryable<T> underlyingQuery,
            MongoContext context,
            params ExpressionVisitor[] visitors)
        {
            Func<Expression, Expression>[] visitFuncs =
                visitors
                .Select(v => (Func<Expression, Expression>)v.Visit)
                .ToArray();
            return Intercept<T>(underlyingQuery, context, visitFuncs);
        }

        private static IQueryable<T> Intercept<T>(
            IQueryable<T> underlyingQuery,
            MongoContext context,
            params Func<Expression, Expression>[] visitors)
        {
            InterceptingProvider provider = new InterceptingProvider(
                underlyingQuery.Provider,
                context,
                visitors
            );
            return provider.CreateQuery<T>(
                underlyingQuery.Expression);
        }
        public IEnumerator<TElement> ExecuteQuery<TElement>(
            Expression expression)
        {
            return _underlyingProvider.CreateQuery<TElement>(
                InterceptExpr(expression)
            ).GetEnumerator();
        }
        public IQueryable<TElement> CreateQuery<TElement>(
            Expression expression)
        {
            return new InterceptedQuery<TElement>(this, expression, _context);
        }
        public IQueryable CreateQuery(Expression expression)
        {
            Type et = TypeHelper.FindIEnumerable(expression.Type);
            Type qt = typeof(InterceptedQuery<>).MakeGenericType(et);
            object[] args = new object[] { this, expression, _context };

            ConstructorInfo ci = qt.GetTypeInfo().GetConstructor(
                new Type[] {
                typeof(InterceptingProvider),
                typeof(Expression)
            });

            return (IQueryable)ci.Invoke(args);
        }
        public TResult Execute<TResult>(Expression expression)
        {
            var result = this._underlyingProvider.Execute<TResult>(
                InterceptExpr(expression)
            );

            if (result != null)
            {
                if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(typeof(TResult)) && !typeof(String).GetTypeInfo().IsAssignableFrom(typeof(TResult)))
                {
                    var list = (IEnumerable)result;

                    foreach (var item in list)
                    {
                        if (typeof(IObjectId).GetTypeInfo().IsAssignableFrom(item.GetType()))
                            this._context.AddObjectToTracking(item as IObjectId);
                    }
                }
                else if (typeof(TResult).GetTypeInfo().IsClass)
                    if (typeof(IObjectId).GetTypeInfo().IsAssignableFrom(result.GetType()))
                        this._context.AddObjectToTracking(result as IObjectId);
            }

            return result;
        }
        public object Execute(Expression expression)
        {
            return this._underlyingProvider.Execute(
                InterceptExpr(expression)
            );
        }
        private Expression InterceptExpr(Expression expression)
        {
            Expression exp = expression;
            foreach (var visitor in _visitors)
                exp = visitor(exp);
            return exp;
        }
    }
}
