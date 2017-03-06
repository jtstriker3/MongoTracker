using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using System.Linq.Expressions;
using MongoDB.Bson;
using System.Reflection;
using System.Security.Cryptography;

namespace MongoTracker.Utility.Extensions
{
    public static class Helpers
    {
        public static T Clone<T>(this T source)
        {
            if (!typeof(T).GetTypeInfo().IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            //Update to work with .net core
            var bson = source.ToBson();
            return MongoDB.Bson.Serialization.BsonSerializer.Deserialize<T>(bson);

            //IFormatter formatter = new BinaryFormatter();
            //Stream stream = new MemoryStream();
            //using (stream)
            //{
            //    formatter.Serialize(stream, source);
            //    stream.Seek(0, SeekOrigin.Begin);
            //    return (T)formatter.Deserialize(stream);
            //}
        }

        public static IEnumerable<Object> GetIds<T>(this T source)
        {
            var properties = typeof(T).GetTypeInfo().GetProperties();
            var id = properties.SingleOrDefault(prop => String.Equals(prop.Name, "id", StringComparison.OrdinalIgnoreCase));

            if (id != null)
                yield return id.GetValue(source);
            else
            {
                var keyProperties = properties.Where(prop => prop.GetCustomAttributes(typeof(BsonIdAttribute), true).Count() > 0);

                foreach (var prop in keyProperties)
                    yield return prop.GetValue(source);
            }

        }

        public static IEnumerable<BsonIdPropertyAndValue> GetIdsAndValues<T>(this T source)
        {
            var properties = typeof(T).GetTypeInfo().GetProperties();
            var id = properties.SingleOrDefault(prop => String.Equals(prop.Name, "id", StringComparison.OrdinalIgnoreCase));

            if (id != null)
                yield return new BsonIdPropertyAndValue(id, id.GetValue(source));
            else
            {
                var keyProperties = properties.Where(prop => prop.GetCustomAttributes(typeof(BsonIdAttribute), true).Count() > 0);

                foreach (var prop in keyProperties)
                    yield return new BsonIdPropertyAndValue(prop, prop.GetValue(source));
            }

        }

        public static String IdToString(this IEnumerable<Object> ids)
        {
            var builder = new StringBuilder();
            foreach (var id in ids)
                builder.Append(id.ToString());

            return builder.ToString();
        }

        public static Object CreateInstance(this Type t, params Object[] args)
        {
            List<Type> types = new List<Type>();
            List<Expression> expressionArgs = new List<Expression>();
            foreach (var arg in args)
            {
                types.Add(arg.GetType());
                expressionArgs.Add(Expression.Constant(arg));
            }

            var constructor = t.GetTypeInfo().GetConstructor(types.ToArray());

            var lambda = Expression.Lambda(
                Expression.Block(
                    Expression.New(constructor, expressionArgs)
                    )
                );

            return lambda.Compile().DynamicInvoke();
        }

        public static String Hash<T>(this T obj)
        {
            var data = obj.ToBson();

            var md5 = MD5.Create();

            var hash = md5.ComputeHash(data);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static IEnumerable<Type> GetTypes(this Type type)
        {
            foreach (var prop in type.GetTypeInfo().GetProperties())
            {
                if (prop.PropertyType.GetTypeInfo().IsGenericType)
                    yield return prop.PropertyType.GetTypeInfo().GetGenericArguments().First();

                yield return prop.PropertyType;
            }
        }

    }

    public class BsonIdPropertyAndValue
    {
        public PropertyInfo KeyInfo { get; set; }
        public Object Value { get; set; }

        public BsonIdPropertyAndValue(PropertyInfo keyInfo, Object value)
        {
            KeyInfo = keyInfo;
            Value = value;
        }
    }
}
