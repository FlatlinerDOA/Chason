namespace Chason
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;

    internal sealed class PropertyParseList<T>
    {
        public static readonly PropertyParseList<T> Instance = new PropertyParseList<T>();

        private readonly IDictionary<string, PropertyParser<T>> setters = new Dictionary<string, PropertyParser<T>>();

        private readonly List<PropertyParser<T>> sequential;

        private IEnumerator<PropertyParser<T>> iterator;

        private readonly Func<T> constructor;

        public PropertyParseList()
        {
            this.constructor = Constructor().Compile();
            var jsonParameter = Expression.Parameter(typeof(JsonParser), "j");
            var instanceParameter = Expression.Parameter(typeof(T), "i");
            var members = from p in typeof(T).GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
                          from c in p.GetCustomAttributes(true).OfType<DataMemberAttribute>()
                          where c != null
                          orderby c.Order, c.Name ?? p.Name
                          select new { Property = p, Contract = c };
            
            this.sequential = new List<PropertyParser<T>>();
            foreach (var m in members)
            {
                var parser = new PropertyParser<T>(m.Contract, m.Property, jsonParameter, instanceParameter);
                this.sequential.Add(parser);
                this.setters.Add(m.Contract.Name ?? m.Property.Name, parser);
            }
        }

        public T New()
        {
            return this.constructor();
        }

        static private Expression<Func<T>> Constructor(params Type[] argTypes)
        {
            ////ParameterExpression[] paramExpressions = new ParameterExpression[argTypes.Length];

            ////for (int i = 0; i < paramExpressions.Length; i++)
            ////{
            ////    paramExpressions[i] = Expression.Parameter(argTypes[i], string.Concat("arg", i));
            ////}

            ConstructorInfo ctorInfo = typeof(T).GetConstructor(argTypes);
            if (ctorInfo == null)
            {
                throw new ArgumentException(String.Concat("The type ", typeof(T).Name, " has no constructor with the argument type(s) ", String.Join(", ", argTypes.Select(t => t.Name).ToArray()), "."),
                        "argTypes");
            }

            return Expression.Lambda<Func<T>>(Expression.New(ctorInfo));
        }

        public void Parse(JsonParser parser, T instance, string name)
        {
            this.iterator = this.iterator ?? this.sequential.GetEnumerator();

            while (iterator.MoveNext())
            {
                if (iterator.Current.Name == name)
                {
                    iterator.Current.Parse(parser, instance);
                    return;
                }
            }

            this.setters[name].Parse(parser, instance);
        }
    }
}