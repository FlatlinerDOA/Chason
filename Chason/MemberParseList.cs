//--------------------------------------------------------------------------------------------------
// <copyright file="MemberParseList.cs" company="Andrew Chisholm">
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

    /// <summary>
    /// Defines the list of properties and their respective parse methods for a given type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the instance with properties being parsed
    /// </typeparam>
    internal sealed class MemberParseList<T>
    {
        private readonly ChasonSerializerSettings settings;

        #region Static Fields

        /// <summary>
        /// The singleton parse list for a given type
        /// </summary>
        public static readonly MemberParseList<T> Instance = new MemberParseList<T>(ChasonSerializerSettings.Default);

        #endregion

        #region Fields

        /// <summary>
        /// The function that creates a new instance of <typeparamref name="T"/>.
        /// </summary>
        private readonly Func<T> constructor;

        /// <summary>
        /// The list of property parsers in the order they are expected to be parsed.
        /// </summary>
        private readonly List<MemberParser<T>> sequential;

        /// <summary>
        /// The setter functions
        /// </summary>
        private readonly IDictionary<string, MemberParser<T>> setters = new Dictionary<string, MemberParser<T>>();

        /// <summary>
        /// The enumerator that iterates over the sequential properties.
        /// </summary>
        private IEnumerator<MemberParser<T>> iterator;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberParseList{T}"/> class.
        /// </summary>
        public MemberParseList(ChasonSerializerSettings settings)
        {
            this.settings = settings;
            this.constructor = Constructor().Compile();
            var jsonParameter = Expression.Parameter(typeof(ChasonParser), "j");
            var instanceParameter = Expression.Parameter(typeof(T), "i");
            var members = Reflect.GetObjectMemberContracts(typeof(T));
            this.sequential = new List<MemberParser<T>>();
            foreach (var m in members)
            {
                var memberType = m.Member.MemberType();
                MemberParser<T> parser;
                if (settings.CustomStringReaders.ContainsKey(memberType))
                {
                    parser = new MemberParser<T>(m.Contract, m.Member, jsonParameter, instanceParameter, settings.CustomStringReaders[memberType]);
                } 
                else if (settings.CustomNumberReaders.ContainsKey(memberType))
                {
                    parser = new MemberParser<T>(m.Contract, m.Member, jsonParameter, instanceParameter, settings.CustomNumberReaders[memberType]);
                }
                else
                {
                    parser = new MemberParser<T>(m.Contract, m.Member, jsonParameter, instanceParameter);
                }
                this.sequential.Add(parser);
                this.setters.Add(m.Contract.Name ?? m.Member.Name, parser);
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

            // TODO: Try resetting the iterator so that rogue unexpected properties don't kill perf.
            // The sequence was unexpected, fall back to a dictionary lookup.
            MemberParser<T> p;
            if (this.setters.TryGetValue(name, out p))
            {
                p.Parse(parser, instance);
            }
            else
            {
                parser.SkipValue();
            }
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