//--------------------------------------------------------------------------------------------------
// <copyright file="PropertyParser{T}.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Encapsulates a single named property and it's corresponding Parse method and Property setter expression.
    /// </summary>
    /// <typeparam name="T">The property type for the property</typeparam>
    internal sealed class PropertyParser<T>
    {
        /// <summary>
        /// Initializes a new instance of the PropertyParser class.
        /// </summary>
        /// <param name="dataMember">The data member declaration for the property, specifying expected order and property name</param>
        /// <param name="info">The property info</param>
        /// <param name="parserParameter">An expression that will reference the <see cref="ChasonParser"/> instance, passed as a parameter</param>
        /// <param name="instanceParameter">An expression that will reference the instance the property is to be assigned to, passed as a parameter</param>
        public PropertyParser(DataMemberAttribute dataMember, PropertyInfo info, ParameterExpression parserParameter, ParameterExpression instanceParameter)
        {
            this.Name = dataMember.Name ?? info.Name;
            this.Sequence = dataMember.Order;
            var parseCall = ChasonParser.GetParseMethodCall(info.PropertyType, parserParameter);
            var setPropertyCall = Expression.Call(instanceParameter, info.GetSetMethod(true), new Expression[] { parseCall });
            this.SetExpression = setPropertyCall;
            this.Parse = Expression.Lambda<Action<ChasonParser, T>>(this.SetExpression, parserParameter, instanceParameter).Compile();
        }

        /// <summary>
        /// Gets or sets the expression to set the property value for the given instance.
        /// </summary>
        public Expression SetExpression { get; set; }

        /// <summary>
        /// Gets or sets the action that will parse the value to assign the property to
        /// </summary>
        public Action<ChasonParser, T> Parse { get; set; }

        /// <summary>
        /// Gets or sets the property's data contract name (or the name that will appear in the JSON)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the expected sequence (while JSON is unordered, having an accurate expected order allows for faster parsing).
        /// </summary>
        public int Sequence { get; set; }
    }
}
