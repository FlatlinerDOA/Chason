namespace Chason
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;

    internal static class PropertyParser
    {
        internal static readonly Dictionary<Type, string> TypeParseMethods = new Dictionary<Type, string> 
        {
            { typeof(string), "ParseString" },
            { typeof(string[]), "ParseStringArray" },
            { typeof(int), "ParseInt32" },
            { typeof(int[]), "ParseInt32Array" },
            { typeof(long), "ParseInt64" },
            { typeof(long[]), "ParseInt64Array" },
            { typeof(decimal), "ParseDecimal" },
            { typeof(decimal[]), "ParseDecimalArray" }
        };

        internal static readonly Dictionary<Type, string> ListParseMethods = new Dictionary<Type, string> 
        {
            { typeof(IList<string>), "ParseStringList" },
            { typeof(IList<int>), "ParseInt32List" },
            { typeof(IList<long>), "ParseInt64List" },
            { typeof(IList<decimal>), "ParseDecimalList" }
        };

        internal static MethodCallExpression GetParseMethodCall(Type type, ParameterExpression parserParameter)
        {
            if (TypeParseMethods.ContainsKey(type))
            {
                return Expression.Call(parserParameter, TypeParseMethods[type], new Type[0]); ;
            }

            if (type.IsGenericType)
            {
                var generic = type.GetGenericTypeDefinition();
                if (typeof(IList<>).IsAssignableFrom(generic))
                {
                    return Expression.Call(parserParameter, "ParseList", new[] { type });
                }
            }

            return Expression.Call(parserParameter, "ParseObject", new[] { type });
        }
    }

    internal sealed class PropertyParser<T>
    {
        public PropertyParser(DataMemberAttribute dataMember, PropertyInfo info, ParameterExpression parserParameter, ParameterExpression instanceParameter)
        {
            this.Name = dataMember.Name ?? info.Name;
            this.Sequence = dataMember.Order;
            var parseCall = PropertyParser.GetParseMethodCall(info.PropertyType, parserParameter);
            var setPropertyCall = Expression.Call(instanceParameter, info.GetSetMethod(true), parseCall);
            this.SetExpression = setPropertyCall;
            this.Parse = Expression.Lambda<Action<JsonParser, T>>(this.SetExpression, parserParameter, instanceParameter).Compile();
        }

        //Action<JsonParser, T>
        public Expression SetExpression { get; set; }

        public Action<JsonParser, T> Parse { get; set; }

        public string Name { get; set; }

        public int Sequence { get; set; }
    }
}