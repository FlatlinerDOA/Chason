//--------------------------------------------------------------------------------------------------
// <copyright file="PropertyParseList.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the list of properties and their respective parse methods for a given type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the instance with properties being parsed
    /// </typeparam>
    internal sealed class PropertyParseList<T>
    {
        #region Static Fields

        /// <summary>
        /// The singleton parse list for a given type
        /// </summary>
        public static readonly PropertyParseList<T> Instance = new PropertyParseList<T>();

        #endregion

        #region Fields

        /// <summary>
        /// The function that creates a new instance of <typeparamref name="T"/>.
        /// </summary>
        private readonly Func<T> constructor;

        /// <summary>
        /// The list of property parsers in the order they are expected to be parsed.
        /// </summary>
        private readonly List<PropertyParser<T>> sequential;

        /// <summary>
        /// The setter functions
        /// </summary>
        private readonly IDictionary<string, PropertyParser<T>> setters = new Dictionary<string, PropertyParser<T>>();

        /// <summary>
        /// The enumerator that iterates over the sequential properties.
        /// </summary>
        private IEnumerator<PropertyParser<T>> iterator;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyParseList{T}"/> class.
        /// </summary>
        public PropertyParseList()
        {
            this.constructor = Constructor().Compile();
            var jsonParameter = Expression.Parameter(typeof(ChasonParser), "j");
            var instanceParameter = Expression.Parameter(typeof(T), "i");
            var members = from p in typeof(T).GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
                          from c in p.GetCustomAttributes(true).OfType<DataMemberAttribute>()
                          where c != null
                          orderby c.Order, c.Name ?? p.Name
                          select new {
                                         Property = p, 
                                         Contract = c
                                     };

            this.sequential = new List<PropertyParser<T>>();
            foreach (var m in members)
            {
                var parser = new PropertyParser<T>(m.Contract, m.Property, jsonParameter, instanceParameter);
                this.sequential.Add(parser);
                this.setters.Add(m.Contract.Name ?? m.Property.Name, parser);
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The method to construct a new instance (turns out this can be faster than the new() constraint) and is more versatile for creating custom types later on.
        /// </summary>
        /// <returns>Returns a new instance of type <typeparamref name="T"/>.</returns>
        public T New()
        {
            return this.constructor();
        }

        /// <summary>
        /// Parses the instance for the given property name.
        /// </summary>
        /// <param name="parser">
        /// The parser instance currently parsing
        /// </param>
        /// <param name="instance">
        /// The instance being deserialized to.
        /// </param>
        /// <param name="name">
        /// The property's data contract name (the property name as read in the JSON).
        /// </param>
        public void Parse(ChasonParser parser, T instance, string name)
        {
            this.iterator = this.iterator ?? this.sequential.GetEnumerator();

            while (this.iterator.MoveNext())
            {
                if (this.iterator.Current.Name == name)
                {
                    this.iterator.Current.Parse(parser, instance);
                    return;
                }
            }

            // TODO: Use TryGetvalue and also try resetting the iterator so that rogue unexpected properties don't kill perf.
            // The sequence was unexpected, fall back to a dictionary lookup.
            this.setters[name].Parse(parser, instance);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a default constructor expression
        /// </summary>
        /// <param name="argTypes">
        /// The types of the arguments to be passed to the constructor
        /// </param>
        /// <returns>
        /// Returns a Lambda expression that returns a new instance.
        /// </returns>
        private static Expression<Func<T>> Constructor(params Type[] argTypes)
        {
            ////ParameterExpression[] paramExpressions = new ParameterExpression[argTypes.Length];

            ////for (int i = 0; i < paramExpressions.Length; i++)
            ////{
            ////    paramExpressions[i] = Expression.Parameter(argTypes[i], string.Concat("arg", i));
            ////}
            ConstructorInfo ctorInfo = typeof(T).GetConstructor(argTypes);
            if (ctorInfo == null)
            {
                throw new ArgumentException(string.Concat("The type ", typeof(T).Name, " has no constructor with the argument type(s) ", string.Join(", ", argTypes.Select(t => t.Name).ToArray()), "."), "argTypes");
            }

            return Expression.Lambda<Func<T>>(Expression.New(ctorInfo));
        }

        #endregion
    }
}